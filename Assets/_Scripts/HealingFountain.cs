using UnityEngine;
using System.Collections;

public class HealingFountain : MonoBehaviour 
{
	//A class for reviving downed players.
	#region vars
	private const float maxJuice = 200.0f;           //how much power can be drained from this font before it moves?
	private float juice;                             //how much power remains?

	private const float regenRate = 20.0f;           //health per second, when down and can't revive
	private const float regenRateTopOff = 10.0f;     //health per second, when down and can revive
	private const float regenRateStillAlive = 10.0f; //health per second, when alive and healing

	private SpriteRenderer spriteRenderer;
	#endregion


	void Awake()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
	}

	// Use this for initialization
	void Start () 
	{
		juice = maxJuice;	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( juice < 0.0f ) { juice = 0.0f; } //minor color correction

		//graphical changes based on power remaining.
		foreach ( ParticleSystem p in GetComponentsInChildren<ParticleSystem>() )
		{
			p.startSize = 1.5f * juice / maxJuice + 1.5f;
			p.emissionRate = 1.5f * juice / maxJuice + 1.5f;
			p.startColor = GetRGBFromHSV ( juice / maxJuice * 120.0f, 1.0f, 1.0f );
		}
		spriteRenderer.color = GetRGBFromHSV ( juice / maxJuice * 120.0f, 1.0f, 1.0f );

		bool stop = false;
		if ( juice <= 0.0f )
		{
			stop = true;
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
							player.GetComponent<PlayerParticleEffectManager>().EnableHealingThisFrame();
							if ( player.HP < player.baseMaxHP * StaticData.percentHPNeededToRevive )
							{
								juice -= Mathf.Min ( regenRate * dt, player.maxHP - player.HP );
								player.HP = Mathf.Min ( player.HP + regenRate * dt, player.maxHP );
							}
							else 
							{
								juice -= Mathf.Min ( regenRateTopOff * dt, player.maxHP - player.HP );
								player.HP = Mathf.Min ( player.HP + regenRateTopOff * dt, player.maxHP );
							}
						}
						else
						{
							//player is alive
							if ( ! stop ) 
							{
								if ( player.channellingHealingCooldown <= 0.0f || player.isChannellingHealing )
								{
									if ( player.HP < player.baseMaxHP )
									{
										player.contextualHealingAvailable = true;
										player.GetComponent<OverHeadText>().SetOverheadText ();
										player.contextualHealingAvailable = false;
									}
								}

								if ( player.isChannellingHealing && ! player.isActuallyHealing )
								{
									if ( player.HP < player.baseMaxHP )
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
									if ( player.HP < player.baseMaxHP )
									{
										juice -= Mathf.Min ( regenRateStillAlive * dt, player.maxHP - player.HP );
										player.HP = Mathf.Min ( player.HP + regenRateStillAlive * dt, player.maxHP );
										player.channellingHealingCooldown = 5.0f; //continually reset cooldown.
										//state
										player.state = "healing";
										player.stateTimer = 0.0f;
										player.nextState = "healing";
										player.canMove = false;
									}
									else
									{
										//you're done healing.
										//end channeling healing
										player.isActuallyHealing = false;
										player.speedMultiplier2 = player.speedMultiplier2 * 2.0f;
										player.channellingHealingCooldown = 5.0f; //reset cooldown.
										player.GetComponent<Animator>().Play ( "throw_" + player.GetAniSuffix() );
										//state
										player.state = "idle";
										player.stateTimer = 0.0f;
										player.nextState = "idle";
										player.canMove = true;
									}
								}
							}
							else if ( player.isActuallyHealing ) //STOP!
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
						}
					}
					//ELSE: box check: get vector of length r pointing to the player.
					// if x component + box width and y component + box height will reach the player (2 dist checks)
				}
			}
		}
		if ( stop )
		{
			float angle = Random.Range ( 0.0f, Mathf.PI * 2.0f );
			transform.position = new Vector3( 15.0f * Mathf.Cos ( angle ), 9.0f * Mathf.Sin ( angle ), 0.0f );
			
			juice = maxJuice;
		}
	}

	private Color GetRGBFromHSV( float h, float s, float v )
	{
		//A function to convert HSV into RGB color space.
		//stolen from: http://www.rapidtables.com/convert/color/hsv-to-rgb.htm
		float c = v * s;
		float x = c * ( 1.0f - Mathf.Abs ( (h / 60) % 2 - 1 ) );
		if      ( h >= 0.0f  && h <=  60.0f ) { return new Color( c, x, 0.0f ); }
		else if ( h > 60.0f  && h <= 120.0f ) { return new Color( x, c, 0.0f ); }
		else if ( h > 120.0f && h <= 180.0f ) { return new Color( 0.0f, c, x ); }
		else if ( h > 180.0f && h <= 240.0f ) { return new Color( 0.0f, x, c ); }
		else if ( h > 240.0f && h <= 300.0f ) { return new Color( x, 0.0f, c ); }
		else if ( h > 300.0f && h <= 360.0f ) { return new Color( c, 0.0f, x ); }
		else { return new Color( 0.0f, 0.0f, 0.0f ); }
	}
}
