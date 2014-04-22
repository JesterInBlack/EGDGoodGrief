using UnityEngine;
using System.Collections;

//ATTACK SYSTEM CLASS:
//basically, you input a shape, the amount of damage, and an ID, 
//and it damages all appropriate objects that it intersects.

//TODO: friendly fire flag?, damage attenuation?

public enum PLAYER_IDS { BOSS = -1, ONE = 0, TWO = 1, THREE = 2, FOUR = 3 }; //makes id codes make english sense.

public class HitOptions
{
	//leightweight class for options on hit.
	public bool knockback;   //whether the attack should stun + knockback
	public bool push;        //whether the attack should "push" the player. Like knockback sans stun.
	public bool angerManip;  //whether the attack should have an impact on boss anger or not
	public Vector2 attackOrigin;

	public HitOptions()
	{
		knockback = false;
		angerManip = false;
	}
}

public static class AttackSystem 
{
	public static int GetLayerMask( int id )
	{
		//Returns the correct layer mask for the object the ID belongs to.
		//Boss : id = -1, Hits Player only
		if ( IsEnemy ( id ) ) { return (1 << LayerMask.NameToLayer("Player") ); }
		//Player : id 1 to 4, Hits Boss and Player
		else {  return ( (1 << LayerMask.NameToLayer ("Boss") ) | ( 1 << LayerMask.NameToLayer ("Player") ) ); }
	}

	public static void hitCircle( Vector2 center, float radius, float damage, int id )
	{
		int layerMask = GetLayerMask( id );
		Collider2D[] hits = Physics2D.OverlapCircleAll( center, radius, layerMask );

		foreach ( Collider2D hit in hits )
		{
			Hit ( hit.gameObject, id, damage );
			Knockback( hit.gameObject, id, damage, center ); //eh, what if we don't want it?
		}
	}

	public static Collider2D[] getHitsInCircle( Vector2 center, float radius, int id )
	{
		//Returns all colliders intersecting / overlapping the circle.

		int layerMask = GetLayerMask( id );
		return Physics2D.OverlapCircleAll( center, radius, layerMask );
	}

	public static void hitLineSegment( Vector2 start, Vector2 end, float damage, int id )
	{
		int layerMask = GetLayerMask( id );

		#region DEBUG
		Debug.DrawLine ( start, end, new Color( 0.0f, 1.0f, 0.0f) );
		#endregion

		RaycastHit2D[] hits = Physics2D.LinecastAll( start, end, layerMask );

		foreach ( RaycastHit2D hit in hits )
		{
			HitOptions options = new HitOptions();
			options.angerManip = false;
			options.knockback = false;
			options.push = true;
			options.attackOrigin = start;
			Hit ( hit.collider.gameObject, id, damage, options );
			//Knockback( hit.collider.gameObject, id, damage, start ); //feels assume-ey.

		}
	}

	public static RaycastHit2D[] getHitsOnLineSegment( Vector2 start, Vector2 end, int id )
	{
		//returns all hits on the line segment

		int layerMask = GetLayerMask( id );
		#region DEBUG
		Debug.DrawLine ( start, end, new Color( 0.0f, 1.0f, 0.0f) );
		#endregion
		
		return Physics2D.LinecastAll( start, end, layerMask );
	}

	public static void hitBox( Rect attackBox, float damage, int id )
	{
		#region DEBUG
		Debug.DrawLine ( new Vector3( attackBox.x, attackBox.y ), 
		                 new Vector3( attackBox.x + attackBox.width, attackBox.y ), 
		                 new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x + attackBox.width, attackBox.y ), 
		                new Vector3( attackBox.x + attackBox.width, attackBox.y + attackBox.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x + attackBox.width, attackBox.y + attackBox.height ), 
		                new Vector3( attackBox.x, attackBox.y + attackBox.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( attackBox.x, attackBox.y + attackBox.height ), 
		                new Vector3( attackBox.x, attackBox.y ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		#endregion

		int layerMask = GetLayerMask( id );
		Collider2D[] hits = Physics2D.OverlapAreaAll( new Vector2( attackBox.x, attackBox.y ), 
		                                              new Vector2( attackBox.x + attackBox.width, attackBox.y + attackBox.height),
		                                              layerMask );
		foreach ( Collider2D hit in hits )
		{
			Hit ( hit.gameObject, id, damage );
			//the origin of a box is a nonsensical and arbitrary concept.
			//so, the origin for a box will need to be explicitly passed for knockback.
		}
	}

	public static Collider2D[] getHitsInBox( Rect box, int id )
	{
		//Returns all colliders in the specified box.

		int layerMask = GetLayerMask( id );
		#region DEBUG
		Debug.DrawLine ( new Vector3( box.x, box.y ), 
		                new Vector3( box.x + box.width, box.y ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( box.x + box.width, box.y ), 
		                new Vector3( box.x + box.width, box.y + box.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( box.x + box.width, box.y + box.height ), 
		                new Vector3( box.x, box.y + box.height ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine ( new Vector3( box.x, box.y + box.height ), 
		                new Vector3( box.x, box.y ), 
		                new Color( 0.0f, 1.0f, 0.0f ) );
		#endregion
		return Physics2D.OverlapAreaAll( new Vector2( box.x, box.y ), 
		                                 new Vector2( box.x + box.width, box.y + box.height),
		                                 layerMask );

	}

	
	public static void hitSector( Vector2 pos, float minTheta, float maxTheta, float maxRadius, float damage, int id )
	{
		int layerMask = GetLayerMask( id );
		
		//NOTE: theta is an angle, in DEGREES, >= -360.0f
		float minRadius = 0.0f;
		
		#region DEBUG
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		            pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		            0.0f ), 
		               new Color( 0.0f, 1.0f, 0.0f ) );
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		            pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		            0.0f ), 
		               new Color( 0.0f, 1.0f, 0.0f ) );
		#endregion
		
		//get all in circle, get angle from center to center, check if in range.
		//if not in angle range, check if linecast hits
		
		//if getcomponent player != null, do player stuff
		//if getcomponent boss != null, do boss stuff
		Collider2D[] potentialHits = Physics2D.OverlapCircleAll( pos, maxRadius, layerMask );
		foreach (Collider2D hit in potentialHits )
		{
			//COMMON LOGIC
			float x = hit.gameObject.transform.position.x;
			float y = hit.gameObject.transform.position.y;

			#region Hit Shape Offsets
			BoxCollider2D tempBox       = hit.GetComponent<BoxCollider2D>();
			CircleCollider2D tempCircle = hit.GetComponent<CircleCollider2D>();
			if ( tempBox != null )
			{
				x += tempBox.center.x;
				y += tempBox.center.y;
			}
			else if ( tempCircle != null )
			{
				x += tempCircle.center.x;
				y += tempCircle.center.y;
			}
			#endregion

			float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;
			Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );
			
			if ( angle >= minTheta && angle <= maxTheta )
			{
				//In sector angle range.
				//Debug.Log ( "Hit, normal" );
				Hit( hit.gameObject, id, damage );
				Knockback( hit.gameObject, id, damage, pos );
			}
			else if ( (minTheta + 360.0f) % 360.0f > (maxTheta + 360.0f) % 360.0f )
			{
				//The sign on the angle changed. 
				//So to tell if it's in the sector, 
				//we check that it is not in the complement of the angle swept from min to max
				//(the complement is the angle swept from max to min.)
				if ( ! (angle >= ( (maxTheta + 360.0f) % 360.0f) && angle <= ( (minTheta + 360.0f) % 360.0f) ) )
				{
					//Debug.Log ( "Hit, negative angle" );
					Hit ( hit.gameObject, id, damage );
					Knockback( hit.gameObject, id, damage, pos );
				}
			}
			else
			{
				//possibly no hit: one last check for edge cases.
				//2 linecasts at the edges of the sector.
				Vector2 start = new Vector2( x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
				                            y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ) );
				Vector2 end   = new Vector2( x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
				                            y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ) );
				RaycastHit2D[] edgeHits1 = Physics2D.LinecastAll ( start, end );
				
				start = new Vector2( x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
				                    y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ) );
				end   = new Vector2( x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
				                    y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ) );
				RaycastHit2D[] edgeHits2 = Physics2D.LinecastAll ( start, end );
				
				//Edge case checks
				bool isHit = false; //to prevent multi-hitting w/ narrow edges.
				foreach ( RaycastHit2D lastHit in edgeHits1 )
				{
					if ( lastHit.collider.gameObject.Equals ( hit.gameObject ) )
					{
						//Debug.Log ( "Hit, edge case (min)." ); //does not ignore self hits
						Hit( hit.gameObject, id, damage );
						Knockback( hit.gameObject, id, damage, pos );
						isHit = true;
					}
				}
				
				if ( isHit == false )
				{
					foreach ( RaycastHit2D lastHit in edgeHits2 )
					{
						if ( lastHit.collider.gameObject.Equals ( hit.gameObject ) )
						{
							//Debug.Log ( "Hit, edge case (max)." );
							Hit( hit.gameObject, id, damage );
							Knockback( hit.gameObject, id, damage, pos );
							isHit = true;
						}
					}
				}
			}
			//END COMMON LOGIC
		}
	}

	private static void Hit( GameObject obj, int id, float damage, HitOptions options )
	{
		//version with options!
		float savedAnger = GameState.angerAxis;

		Hit ( obj, id, damage );

		if ( options.angerManip == false )
		{
			GameState.angerAxis = savedAnger;
		}
		if ( options.push == true )
		{
			Push ( obj, id, damage, options.attackOrigin, 0.2f );
		}
	}

	private static void Hit( GameObject obj, int id, float damage )
	{
		//Does damage to a game object (boss / player)
		//Gets implicit info about sender from id (player or boss?)
		//Player -> boss: damage
		//Boss -> player: damage
		//Player -> player: interrupt
		//Boss -> boss: masked out

		//NOTE: players cannot hit themselves.
		
		Player hitPlayer = obj.GetComponent<Player>();
		LegScript hitLeg = obj.GetComponent<LegScript>();
		BossCoreHP hitBoss = obj.GetComponent<BossCoreHP>();
		TetherProjectileScript hitTether = obj.GetComponent<TetherProjectileScript>(); //TODO: replace this with a common add class?
		//TODO: put a hit "add / leg / enemy" in here.
		
		if ( hitPlayer != null ) //hit a player
		{
			if ( IsPlayer ( id ) ) //player -> player attack
			{
				if ( hitPlayer.id != id ) //no self-hitting
				{
					hitPlayer.Interrupt( id, damage );
				}
			}
			else if ( IsEnemy( id ) ) //enemy -> player attack
			{
				hitPlayer.Hurt ( damage );
			}
		}
		else if ( hitLeg != null )
		{
			if ( IsPlayer ( id ) ) //player -> boss attack
			{
				hitLeg.Hurt ( damage, id );
			}
		}
		else if ( hitBoss != null ) //hit the boss
		{
			if ( IsPlayer ( id ) ) //player -> boss attack
			{
				hitBoss.Hurt ( damage, id );
			}
			//else: ignore it.
		}
		else if ( hitTether != null )
		{
			if ( IsPlayer ( id ) )
			{
				hitTether.Hurt ( damage, id );
			}
		}
	}

	private static void Knockback( GameObject obj, int id, float damage, Vector2 attackOrigin )
	{
		Player hitPlayer = obj.GetComponent<Player>();
		if ( hitPlayer != null ) //hit a player
		{
			if ( IsEnemy( id ) ) //enemy -> player attack
			{
				float magnitude = damage / 10.0f; //guess?
				if ( attackOrigin != new Vector2( hitPlayer.transform.position.x, hitPlayer.transform.position.y ) )
				{
					hitPlayer.KnockBack ( magnitude, attackOrigin );
				}
				else
				{
					hitPlayer.KnockBack ( magnitude, new Vector2(GameState.boss.transform.position.x, GameState.boss.transform.position.y) );
				}
			}
			//player -> player attacks can rip positions of the players from gamestate for knockback.
		}
	}

	private static void Push( GameObject obj, int id, float damage, Vector2 attackOrigin, float duration )
	{
		Player hitPlayer = obj.GetComponent<Player>();
		if ( hitPlayer != null ) //hit a player
		{
			if ( IsEnemy( id ) ) //enemy -> player attack
			{
				float magnitude = damage / 10.0f; //guess?
				if ( attackOrigin != new Vector2( hitPlayer.transform.position.x, hitPlayer.transform.position.y ) )
				{
					hitPlayer.Push ( magnitude, attackOrigin, duration );
				}
				else
				{
					hitPlayer.Push ( magnitude, new Vector2(GameState.boss.transform.position.x, GameState.boss.transform.position.y), duration );
				}
			}
			//player -> player attacks can rip positions of the players from gamestate for knockback.
		}
	}

	private static bool IsPlayer( int id )
	{
		//checks if the id belongs to a player.
		return ( id >= 0 && id < 4 );
	}

	private static bool IsEnemy( int id )
	{
		//checks if the id belongs to an enemy.
		return ( id == -1 );
	}

	//Utility functions for special boss attacks.
	public static void Suck( Vector2 position, float speed, float dt )
	{
		//Sucks all players toward position at speed.
		for ( int i = 0; i < 4; i++ )
		{
			if ( ! GameState.players[i].GetComponent<Player>().isDowned ) //don't draw in the downed.
			{
				float x = GameState.players[i].transform.position.x;
				float y = GameState.players[i].transform.position.y;
				float z = GameState.players[i].transform.position.z;
				Vector2 moveDirection = position - new Vector2( x, y );
				moveDirection.Normalize();
				moveDirection = moveDirection * speed * dt;
				GameState.players[i].transform.position = new Vector3( x + moveDirection.x, y + moveDirection.y, z );
			}
		}
	}

	public static void Tether( GameObject player, Vector2 point, float minRange, float maxRange, float dt )
	{
		if ( player.GetComponent<Player>().isDowned ) { return; } //don't pull downed players.

		float PLAYER_BASE_SPEED = StaticData.playerMoveSpeed;
		float x = player.transform.position.x;
		float y = player.transform.position.y;
		//float z = player.transform.position.z;
		Vector2 pos = new Vector2( x, y );
		Vector2 moveDirection = point - pos;
		float dist = moveDirection.magnitude;

		//We want it to pull a diminishing amount (> 100% speed outside range -> 100% speed at edge -> 0% speed at center)
		//We'll cap it if way outside range.
		//float maxRange = 2.0f; //maximum range.
		float multiplier = Mathf.Max ( 0.0f, dist - minRange) / ( maxRange - minRange );
		if ( multiplier > 10.0f ) { multiplier = 10.0f; } //to help prevent overcompensation.

		float xMove = moveDirection.normalized.x * multiplier * PLAYER_BASE_SPEED * dt;
		float yMove = moveDirection.normalized.y * multiplier * PLAYER_BASE_SPEED * dt;
		//player.transform.position = new Vector3( x + xMove, y + yMove, z );
		player.GetComponent<CustomController>().MoveNaoPlz ( new Vector3(xMove, yMove, 0.0f) ); //respect wall collisions
	}
}
