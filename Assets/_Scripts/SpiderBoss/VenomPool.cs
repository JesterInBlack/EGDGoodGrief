using UnityEngine;
using System.Collections;

public class VenomPool : MonoBehaviour 
{
	#region vars
	private const float degenRate = 3.0f;       //health per second from standing in the pool.
	private const float poisonRate = 0.5f;      //how many seconds pass between each debuff stack application.
	private const float poisonDamage = 0.125f;  //Secondary effect DPS
	private const float poisonDuration = 8.0f;  //Secondary effect duration
	private const float lifespan = 10.0f;       //lifespan (in s)
	private const float maxSize = 1.75f;

	//timers
	private float poisonT = 0.0f;
	private float lifetimer = 0.0f;

	private float growInTime = 0.5f;
	private float fadeOutTime = 0.75f;
	#endregion

	// Use this for initialization
	void Start () 
	{
		transform.localScale = new Vector3(0.0f, 0.0f, 0.0f); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		//grow in.
		float temp = Mathf.Lerp ( 0.0f, maxSize, Mathf.Min( 1.0f, lifetimer / growInTime) );
		transform.localScale = new Vector3( temp, temp, temp );
		
		//fade out
		temp = Mathf.Lerp( 1.0f, 0.0f, Mathf.Min ( 1.0f, (lifetimer - (lifespan - fadeOutTime)) / fadeOutTime ) );
		this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, temp );

		#region timer
		bool applyDebuff = false;
		float dt = Time.deltaTime * StaticData.t_scale;
		lifetimer += dt;
		poisonT += dt;
		if ( poisonT > poisonRate )
		{
			applyDebuff = true;
			poisonT -= poisonRate;
		}

		if ( lifetimer >= lifespan )
		{
			Destroy( this.gameObject );
		}
		#endregion

		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				Player player = GameState.playerStates[i];
				//get distance from player's center to the hit circle's center
				BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
				Vector3 theirPos = player.gameObject.transform.position;
				CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
				Vector3 myPos = this.gameObject.transform.position;
				float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
				float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
				float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );
				
				if ( dist <= circle.radius * transform.localScale.x ) //if player is in the region (collider center point in radius)
				{
					if ( ! player.isCarried ) //if the player is not being carried
					{
						if ( ! player.isDowned )
						{
							//Degenerate health!
							if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; }
							player.HP = Mathf.Max ( player.HP - (degenRate * dt / player.defense), 0.0f );
							if ( applyDebuff )
							{
								player.Poison ( poisonDuration, poisonDamage );
							}
						}
					}
				}
			}
		}
	}
}
