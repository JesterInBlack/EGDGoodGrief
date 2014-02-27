using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public class RocketSwordFunctions : MonoBehaviour, ClassFunctionalityInterface 
{
	//This class defines the moves for the rocket sword class.
	//It reaches into the base player class to modify certain attributes.
	//TODO: add grace time resets to moves.

	#region vars
	private Player player;
	private CustomController controller;

	//button hold times
	private float xHoldTime  = 0.0f;
	private const float xChargeMin = 1.0f;    //minimum hold time to use the charged version of the x attack
	private const float xChargeMax = 10.0f;   //maximum hold time: more than this confers no benefit.
	private const float xNormalGraceT = 1.0f; //Grace time before chain degeneration happens.
	//private const float xSmashGraceT = 1.0f;  //Grace time before chain degeneration happens.
	private float yHoldTime  = 0.0f;
	private const float yChargeMin = 1.0f;    //minimum hold time to use the charged version of the y attack
	private const float yChargeMax = 10.0f;   //maximum hold time: more than this confers no benefit.
	private const float yNormalGraceT = 1.0f; //Grace time before chain degeneration happens.
	//private const float ySmashGraceT = 1.0f;  //Grace time before chain degeneration happens.
	private float bHoldTime  = 0.0f;
	private float rtHoldTime = 0.0f;

	private int maxSpinToWinExtensions = 5; //the maximum number of times spin2win can be extended
	private int spinToWinExtensions = 0;    //the number of times spin2win has been extended
	private float spinTime = 0.0f;			//how long you've been spinning for.
	private float attackDamage = 0.0f;      //damage the current attack will deal.

	private const float parryTime = 0.5f; //how long the parry lasts for. (in s)

	private string prevState = "";
	#endregion

	// Use this for initialization
	void Start () 
	{
		player = GetComponent<Player>();
		controller = GetComponent<CustomController>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region dt
		float dt = Time.deltaTime;
		if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; }
		#endregion

		if ( player.state == "xnormal" )
		{
			//do sector based on time in state + direction
			float angle = 90.0f * GetComponent<CustomController>().facing;
			float damage = 10.0f;
			hitSector ( new Vector2( transform.position.x, transform.position.y ), -67.5f + angle, 67.5f + angle, 0.1f, 2.0f, damage * dt );
		}
		if ( player.state == "xsmash" )
		{
			//do sector based on time in state + direction
			//change sector angle based on frame: 1/20 s = 1/15 rotation (24 degrees)
			spinTime += dt;
			float minAngle = ( ( ( ( 180.0f - spinTime * 360.0f / (0.75f / 2.0f) ) % 360.0f) + 360.0f) % 360.0f);
			float maxAngle = ( ( ( ( minAngle + 90.0f ) % 360.0f) + 360.0f) % 360.0f);
			hitSector ( new Vector2( transform.position.x, transform.position.y ), minAngle, maxAngle, 0.1f, 2.0f, attackDamage * dt );
			/*
			if ( spinTime % (0.75f / 2.0f) != spinTime )
			{
				spinTime = spinTime % (0.75f / 2.0f );
				hitSector ( new Vector2( transform.position.x, transform.position.y ), 0.01f, 360.0f, 0.1f, 2.0f );
			}
			*/
		}

		if ( player.state == "ynormal" )
		{
			//hit objects in the hitbox
		}
		if ( player.state == "ysmash" )
		{
			//charge until duration runs out / you hit something (other than another player?)
			float angle = GetComponent<CustomController>().facing * Mathf.PI / 2;
			float speed = 13.0f * dt;
			GetComponent<CustomController>().MoveNaoPlz( new Vector3( speed * Mathf.Cos ( angle ), speed * Mathf.Sin ( angle ), 0.0f ) );
		}

		if ( player.state == "idle" )
		{
			#region animation
			if ( GetComponent<CustomController>().facing == 0 )
			{
				GetComponent<Animator>().Play ( "idle_right" );
			}
			else if ( GetComponent<CustomController>().facing == 1 )
			{
				GetComponent<Animator>().Play ( "idle_up" );
			}
			else if ( GetComponent<CustomController>().facing == 2 )
			{
				GetComponent<Animator>().Play ( "idle_left" );
			}
			else if ( GetComponent<CustomController>().facing == 3 )
			{
				GetComponent<Animator>().Play ( "idle_down" );
			}
			#endregion
		}

		//handle odd state transitions?
		//(chains longer than 2?)
		if ( prevState != player.state )
		{
			OnStateChange( player.state );
		}
		prevState = player.state;
		//Debug.Log ( player.state ); //DEBUG
	}

	void hitSector( Vector2 pos, float minTheta, float maxTheta, float minRadius, float maxRadius, float damage )
	{
		//NOTE: theta is an angle, in DEGREES, >= -360.0f

		#region DEBUG
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * minTheta ), 
		            				pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * minTheta ), 
		            				0.0f ), 
		               new Color(0.0f, 1.0f, 0.0f) );
		Debug.DrawLine( new Vector3( pos.x + minRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		                            pos.y + minRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		                            0.0f ), 
		               new Vector3( pos.x + maxRadius * Mathf.Cos ( Mathf.Deg2Rad * maxTheta ), 
		            				pos.y + maxRadius * Mathf.Sin ( Mathf.Deg2Rad * maxTheta ), 
		            				0.0f ), 
		               new Color(0.0f, 1.0f, 0.0f) );
		#endregion

		#region players
		//players
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				float x = GameState.players[i].transform.position.x;
				float y = GameState.players[i].transform.position.y;
				float dist = Mathf.Pow(  ( (pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y) ), 0.5f );

				if ( dist >= minRadius && dist <= maxRadius )
				{
					float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;

					//Debug.Log ( angle ); //DEBUG
					Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );

					if ( angle >= minTheta && angle <= maxTheta )
					{
						//Debug.Log ( "Hit, normal" );
						GameState.players[i].GetComponent<Player>().Hurt ( damage );
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
							GameState.players[i].GetComponent<Player>().Hurt ( damage );
						}
					}
				}
			}
		}
		#endregion
		#region boss
		//boss
		if ( GameState.boss != null )
		{
			float x = GameState.boss.transform.position.x;
			float y = GameState.boss.transform.position.y;
			float dist = Mathf.Pow(  ( (pos.x - x) * (pos.x - x) + (pos.y - y) * (pos.y - y) ), 0.5f );
			
			if ( dist >= minRadius && dist <= maxRadius )
			{
				float angle = (Mathf.Rad2Deg * Mathf.Atan2 ( y - pos.y, x - pos.x ) + 360.0f) % 360.0f;

				Debug.DrawLine( new Vector3( pos.x, pos.y, 0.0f ), new Vector3( x, y, 0.0f ), new Color(1.0f, 0.0f, 0.0f) );
				
				if ( angle >= minTheta && angle <= maxTheta )
				{
					GameState.boss.GetComponent<Boss>().Hurt ( damage );
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
						GameState.boss.GetComponent<Boss>().Hurt ( damage );
					}
				}
			}
		}
		#endregion
	}

	#region B
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
		//Initialize parry.
		//Probably just animation + state vars
		//Can't parry while attacking?
		//Can parry out of default states + charging
		if ( player.state == "idle" || player.state == "walk" || player.state == "revcharge" )
		{
			player.canMove = false;
			player.isParrying = true;
			player.parryTimer = parryTime;
		}
	}
	public void BReleased()
	{
		//Called when B is released.
		//DO NOTHING.
		bHoldTime = 0.0f;
	}
	public void BHeld( float dt )
	{
		//Called every frame B is held down.
		//DO NOTHING.
		bHoldTime += dt;
	}
	#endregion
	#region X
	//Tap / Hold
	public void XPressed()
	{
		//Called when X is pressed.
		//Initialize horizontal slash.
		//OR: prolong hurricane spin.
		if ( player.state == "xsmash" )
		{
			if ( spinToWinExtensions < maxSpinToWinExtensions )
			{
				//make it so mashing x doesn't extend it multiple times...
				//that is, pressing x 5 times during the first spin cycle shouldn't make you do all 6 attacks. 
				//this is to keep it feeling fluid.
				float t = 0.05f * 15.0f; //time to do one spin.
				if ( player.stateTimer <= t )
				{
					spinToWinExtensions++;
					player.stateTimer += t;
				}
			}
		}

		//Animation + state vars
		if ( player.state == "idle" || player.state == "walk" || player.state == "revcharge" )
		{
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.interruptHP = 100.0f;
			player.state = "xwindup";
			player.stateTimer = 0.05f * 7.0f;
			player.nextState = "xcharge";
			//use combo timing to shorten animation cycle between attacks.
			//pressing x or y while the attack / recovery is going -> queues up a 0 windup attack.
			#region animation
			if ( GetComponent<CustomController>().facing == 0 )
			{
				GetComponent<Animator>().Play ( "xslash_right_windup" );
			}
			else if ( GetComponent<CustomController>().facing == 1 )
			{
				GetComponent<Animator>().Play ( "xslash_up_windup" );
			}
			else if ( GetComponent<CustomController>().facing == 2 )
			{
				GetComponent<Animator>().Play ( "xslash_left_windup" );
			}
			else if ( GetComponent<CustomController>().facing == 3 )
			{
				GetComponent<Animator>().Play ( "xslash_down_windup" );
			}
			#endregion
		}
		else if ( player.state == "xwinddown" || player.state == "ywinddown" ) //cut off frames if you attack during recovery
		{
			player.nextState = "xcharge";
			player.interruptHP = 100.0f;
		}

	}
	public void XReleased()
	{
		//Called when X is released.
		//Do horizontal slash / hurricane spin.

		//Animation + state vars
		//poll gamestate, check for sector hit detection
		//hurricane spin makes the sector thing a bit more complex.
		//would it be better to poll gamestate, or try physics raycast type deal?
		if ( player.state != "xwindup" && player.state != "xcharge" && player.state != "xwinddown") { return; }
		if (player.state == "xwinddown" )
		{
			//released early, queue up normal slash
			player.nextState = "xnormal";
		}
		if ( player.state == "xwindup" )
		{
			//released early, queue up normal slash
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			player.nextState = "xnormal";
		}
		if ( player.state == "xcharge" )
		{
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			if ( xHoldTime < xChargeMin )
			{
				//Horizontal attack
				player.nextState = "xnormal";
			}
			else
			{
				//Spin2Win
				player.canMove = true;
				player.state = "xsmash";
				player.stateTimer = 0.05f * 15.0f;
				player.nextState = "idle";
				//get power based on charge and resource.
				float chargePercent = Mathf.Min ( xHoldTime, xChargeMax ) / xChargeMax; //0.0f - 1.0f
				float chainPercent = player.resource; //0.0f - 1.0f
				//full charge = +100% damage, full chain = +200% damage.
				//multiplicative stacking?
				attackDamage = 100.0f * 1.0f * player.offense * (1.0f + 2.0f * chainPercent) * (1.0f + 1.0f * chargePercent);
				player.resource = 0.0f;
				spinTime = 0.0f;
				spinToWinExtensions = 0;
				player.interruptHP = 1000000.0f; //uninterruptable, for all intents and purposes.
				GetComponent<Animator>().Play( "hurricane_spin" );
				//maxSpinToWinExtensions = 5; //Scale with charge?
				//Mathf.Min ( xHoldTime, xChargeMax ) / xChargeMax;
			}
		}
		xHoldTime = 0.0f;
	}

	public void XHeld( float dt )
	{
		//Called every frame X is held down.
		//Charge horizontal slash -> hurricane spin.

		//Animation + state vars
		xHoldTime += dt;
		if ( player.state == "xcharge" )
		{
			//freeze animation at charging.

			if ( xHoldTime - dt < xChargeMin && xHoldTime >= xChargeMin )
			{
				//feedback: charge reached.
			}
			if ( xHoldTime - dt < xChargeMax && xHoldTime >= xChargeMax )
			{
				//feedback: max charge reached.
			}
		}
	}
	#endregion
	#region Y
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
		//Initialize overhead strike.
		if ( player.state == "idle" || player.state == "walk" || player.state == "revcharge" )
		{
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.interruptHP = 100.0f;
			player.state = "y";
			player.stateTimer = 0.0f;
			player.nextState = "y";
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		//Do overhead strike / blastoff
		//Animation + state vars
		//poll gamestate, do hitbox hit detection.
		//with the charge, hitbox detection over time. A bit more complex?
		if ( player.state != "y" ) { return; }
		if ( yHoldTime < yChargeMin )
		{
			//overhead strike
			player.canMove = false;
			player.state = "ynormal";
			player.stateTimer = 0.25f;
			player.nextState = "idle";
			player.resourceGraceT = yNormalGraceT;
			//TODO: box to box collision detection
		}
		else
		{
			//Blast Off
			player.state = "ysmash";
			player.nextState = "idle";
			player.canMove = false;//can't use move if I do this.
			//get power based on charge and resource.
			float chargePercent = Mathf.Min ( yHoldTime, yChargeMax ) / yChargeMax; //0.0f - 1.0f
			float chainPercent = player.resource; //0.0f - 1.0f
			//full charge = +100% damage, full chain = +200% damage.
			//multiplicative stacking?
			attackDamage = 1.0f * player.offense * (1.0f + 2.0f * chainPercent) * (1.0f + 1.0f * chargePercent);
			player.stateTimer = 1.0f + 1.0f * chargePercent;
			player.resource = 0;
			//TODO: hitbox to hitbox collision detection.
		}
		player.speedMultiplier = player.speedMultiplier * 2.0f;
		yHoldTime = 0.0f;
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
		//Charge overhead strike -> blastoff
		yHoldTime += dt;
		if ( player.state == "y" )
		{
			if ( yHoldTime - dt < yChargeMin && yHoldTime >= yChargeMin )
			{
				//feedback: charge reached.
			}
			if ( yHoldTime - dt < yChargeMax && yHoldTime >= yChargeMax )
			{
				//feedback: max charge reached.
			}
		}
	}
	#endregion
	#region RT
	//Tap / Hold
	public void RTPressed()
	{
		//Called when RT is pressed.
		//Initialize rev charge.
		//Slow speed.
		if ( player.state == "idle" || player.state == "walk" )
		{
			player.state = "revcharge";
			player.stateTimer = 0.0f;
			player.nextState = "revcharge";
			player.speedMultiplier = player.speedMultiplier * 0.5f;
		}
	}
	public void RTReleased()
	{
		//Called when RT is released.
		//Set up no degen window.
		//Reset speed.
		if ( player.state == "revcharge" )
		{
			player.state = "idle";
			player.stateTimer = 0.0f;
			player.nextState = "idle";
			player.speedMultiplier = player.speedMultiplier * 2.0f;
		}
		rtHoldTime = 0.0f;
		GamePad.SetVibration ( controller.playerIndex, 0.0f, 0.0f );
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
		//Increase rev charge slightly?
		if ( player.state == "revcharge" )
		{
			//1.0f * dt = 100% per second
			float deltaResource = ( 0.25f + 0.5f * Mathf.Sin ( rtHoldTime * Mathf.PI * 2.0f ) ) * 0.20f * dt; //0.10f * dt;
			player.resource = Mathf.Min ( player.resource + deltaResource, 1.0f );
			player.resourceGraceT = 0.5f;
			GamePad.SetVibration ( controller.playerIndex, 
			                      0.05f + 0.05f * (0.5f * ( Mathf.Sin( rtHoldTime * Mathf.PI * 2.0f ) + 1.0f ) ), 
			                      0.1f + 0.1f * (0.5f * ( Mathf.Cos( rtHoldTime * Mathf.PI * 2.0f ) + 1.0f ) ) );
		}
		else
		{
			GamePad.SetVibration ( controller.playerIndex, 0.0f, 0.0f );
		}
		rtHoldTime += dt;
	}
	#endregion

	void OnStateChange( string newState )
	{
		//handles setting up chains of state changes.
		#region X
		if ( newState == "xnormalwindup" )
		{
			player.nextState = "xcharge";
			player.stateTimer = 0.05f * 7.0f; //7 frame windup
		}
		else if ( newState == "xcharge" )
		{
			player.nextState = "xcharge"; //freeze at this state
		}
		else if ( newState == "xnormal" )
		{
			//initialize the attack.
			player.canMove = false;
			player.stateTimer = 0.05f * 5.0f;
			player.nextState = "xwinddown";//"idle";
			player.canMove = false;
			player.resource = Mathf.Min ( player.resource + 1.0f / 8.0f, 1.0f );
			player.resourceGraceT = xNormalGraceT;
			player.interruptHP = 100.0f;
			#region animation
			if ( GetComponent<CustomController>().facing == 0 )
			{
				gameObject.GetComponent<Animator>().Play( "xslash_right" );
			}
			else if ( GetComponent<CustomController>().facing == 1 )
			{
				gameObject.GetComponent<Animator>().Play( "xslash_up" );
			}
			else if ( GetComponent<CustomController>().facing == 2 )
			{
				gameObject.GetComponent<Animator>().Play( "xslash_left" );
			}
			else if ( GetComponent<CustomController>().facing == 3 )
			{
				gameObject.GetComponent<Animator>().Play( "xslash_down" );
			}
			#endregion
		}
		else if ( newState == "xwinddown" )
		{
			player.nextState = "xwinddown2";
			player.stateTimer = 0.05f * 5.0f; //5 frame recovery
		}
		else if ( newState == "xwinddown2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 6.0f; //6 frame recovery
		}
		else if ( newState == "xsmash" )
		{

		}
		#endregion
		else if ( newState == "ynormal" )
		{
		}
		else if ( newState == "ysmash" )
		{

		}
		else if ( newState == "walk" )
		{
			#region animation
			if ( GetComponent<CustomController>().facing == 0 )
			{
				gameObject.GetComponent<Animator>().Play( "walk_right" );
			}
			else if ( GetComponent<CustomController>().facing == 1 )
			{
				gameObject.GetComponent<Animator>().Play( "walk_up" );
			}
			else if ( GetComponent<CustomController>().facing == 2 )
			{
				gameObject.GetComponent<Animator>().Play( "walk_left" );
			}
			else if ( GetComponent<CustomController>().facing == 3 )
			{
				gameObject.GetComponent<Animator>().Play( "walk_down" );
			}
			#endregion
		}
	}
}
