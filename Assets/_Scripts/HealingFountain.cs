using UnityEngine;
using System.Collections;

public class HealingFountain : MonoBehaviour 
{
	//A class for reviving downed players.
	#region vars
	private const float maxJuice = 250.0f;           //how much power can be drained from this font before it moves?
	private float juice;                             //how much power remains?

	private const float regenRate = 20.0f;           //health per second, when down and can't revive
	private const float regenRateTopOff = 10.0f;     //health per second, when down and can revive
	private const float regenRateStillAlive = 10.0f; //health per second, when alive and healing
	#endregion


	// Use this for initialization
	void Start () 
	{
		juice = maxJuice;	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//TODO: graphical changes based on power remaining.
		if ( juice <= 0.0f )
		{
			//move!
			juice = maxJuice;
		}

		//look through players, heal those in range.
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			Player player = GameState.playerStates[i];
			if ( player != null ) 
			{
				//get distance from player's center to the hit circle's center
				BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
				Vector3 theirPos = player.gameObject.transform.position;
				CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
				Vector3 myPos = this.gameObject.transform.position;
				float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
				float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
				float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );

				if ( dist <= circle.radius ) //if player is in the region (collider center point in radius)
				{
					if ( ! player.isCarried ) //if the player is not being carried
					{
						//Regenerate health!
						float dt = Time.deltaTime;
						if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; }
						//player.HP = Mathf.Min ( player.HP + regenRate * dt, player.maxHP );
						//TODO: make up players able to channel heals on a cooldown.

						if ( player.isDowned )
						{
							//player is dead
							//regenerate max hp!
							player.maxHP = Mathf.Min ( player.maxHP + regenRate * dt, player.baseMaxHP );
							if ( player.HP < player.baseMaxHP * StaticData.percentHPNeededToRevive )
							{
								juice -= Mathf.Min ( player.HP + regenRate * dt, player.maxHP - player.HP );
								player.HP = Mathf.Min ( player.HP + regenRate * dt, player.maxHP );
							}
							else 
							{
								juice -= Mathf.Min ( player.HP + regenRateTopOff * dt, player.maxHP - player.HP );
								player.HP = Mathf.Min ( player.HP + regenRateTopOff * dt, player.maxHP );
							}
						}
						else
						{
							//player is alive
							if ( player.channellingHealingCooldown <= 0.0f || player.isChannellingHealing )
							{
								player.contextualHealingAvailable = true;
								player.GetComponent<OverHeadText>().SetOverheadText ();
								player.contextualHealingAvailable = false;
							}

							if ( player.isChannellingHealing && ! player.isActuallyHealing )
							{
								//begin channelling healing
								player.isActuallyHealing = true;
								player.speedMultiplier2 = player.speedMultiplier2 * 0.5f;
								player.channellingHealingCooldown = 5.0f; //reset cooldown.
								//state
								player.state = "healing";
								player.stateTimer = 0.0f;
								player.nextState = "healing";
								player.GetComponent<Animator>().Play ( "pickup_" + player.GetAniSuffix() );
							}
							else if ( player.isActuallyHealing && ! player.isChannellingHealing )
							{
								//end channeling healing
								player.isActuallyHealing = false;
								player.speedMultiplier2 = player.speedMultiplier2 * 2.0f;
								player.channellingHealingCooldown = 5.0f; //reset cooldown.
								//state
								player.state = "idle";
								player.stateTimer = 0.0f;
								player.nextState = "idle";
								player.canMove = true;
							}
							else if ( player.isActuallyHealing )
							{
								//you're healing.
								juice -= Mathf.Min ( player.HP + regenRateStillAlive * dt, player.maxHP - player.HP );
								player.HP = Mathf.Min ( player.HP + regenRateStillAlive * dt, player.maxHP );
								player.channellingHealingCooldown = 5.0f; //continually reset cooldown.
								//state
								player.state = "healing";
								player.stateTimer = 0.0f;
								player.nextState = "healing";
								player.canMove = false;
							}
						}
					}
				}
				//ELSE: box check: get vector of length r pointing to the player.
				// if x component + box width and y component + box height will reach the player (2 dist checks)
			}
		}
	}
}
