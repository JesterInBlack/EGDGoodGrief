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

	private bool ignoreOnHitCallback = true;

	//button hold times + move stats
	#region move data

	#region X
	private const float xNormalBaseDamage = 100.0f;   //Normal attack: Horizontal Slash: base damage             (DPS)
	private const float xSmashBaseDamage  = 100.0f;   //Smash  attack: Spin2Win: base damage (0 charge)          (DPS)
	private const float xSmashAddDamage   = 150.0f;   //Smash  attack: Spin2Win: additional damage (100% charge) (DPS)
	private const float xSmashChainBonus  = 3.0f;     //Smash  attack: Spin2Win: damage multiplier from full chain

	private const float xNormalAngle = 115.0f;        //Normal attack: Horizontal Slash: hit sector angle.

	private const float xChargeInterruptHP = 100.0f;  //Charging up X: interruption damage threshold 
	private const float xNormalInterruptHP = 100.0f;  //Normal attack: Horizontal Slash: interruption damage threshold.
	private const float xSmashInterruptHP = 10000.0f; //Smash  attack: Spin2Win: interruption damage threshold.

	public bool xCharged = false;                     //for external scripts to access
	public bool xCharged2 = false;                    //for external scripts to access
	private bool xSmashUsed = false;                  //set to reset speed proper-like
	private float xHoldTime  = 0.0f;
	private const float xChargeMin = 1.0f;            //minimum hold time to use the charged version of the x attack
	private const float xChargeMax = 5.0f;            //maximum hold time: more than this confers no benefit.
	//private const float xNormalGraceT = 1.0f;       //Grace time before chain degeneration happens.
	//private const float xSmashGraceT = 1.0f;        //Grace time before chain degeneration happens. Moot.

	private int maxSpinToWinExtensions = 5;           //the number of times spin2win can be extended (based on charge)
	private int spinToWinExtensions = 0;              //the number of times spin2win has been extended
	private float spinTime = 0.0f;			          //how long you've been spinning for.
	#endregion
	
	#region Y
	private const float yNormalBaseDamage = 150.0f;   //Normal attack: Vertical Slash: base damage                (Single Hit)
	private const float ySmashBaseDamage  = 150.0f;   //Smash  attack: Blast Off: base damage (0 charge)          (DPS)
	private const float ySmashAddDamage   = 100.0f;   //Smash  attack: Blast Off: additional damage (100% charge) (DPS)
	private const float ySmashChainBonus  = 3.0f;     //Smash  attack: Blast Off: damage multiplier from full chain

	private const float yChargeInterruptHP = 100.0f;  //Charging up Y: interruption damage threshold.
	private const float yNormalInterruptHP = 100.0f;  //Normal attack: Vertical Slash: interruption damage threshold.
	private const float ySmashInterruptHP  = 100.0f;  //Smash  attack: HBlast Off: interruption damage threshold.
	private bool ySmashHalted = false;                //set to true if the y smash connects. It stops the motion.

	public bool yCharged = false;                     //for external scripts to access
	private float yHoldTime  = 0.0f;
	private const float yChargeMin = 1.0f;            //minimum hold time to use the charged version of the y attack
	private const float yChargeMax = 5.0f;            //maximum hold time: more than this confers no benefit.
	//private const float yNormalGraceT = 1.0f;       //Grace time before chain degeneration happens.
	//private const float ySmashGraceT = 1.0f;        //Grace time before chain degeneration happens. Moot.
	#endregion

	#region Parry
	private const float parryTime = 0.5f;             //how long the parry lasts for. (in s)
	#endregion

	#endregion

	private float bHoldTime  = 0.0f;
	private float rtHoldTime = 0.0f;

	private float attackDamage = 0.0f;                //damage the current attack will deal.
	private const float GraceT = 1.0f;                //how long you have after an attack (in s) before resource degen starts.

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
		UpdateResource ( dt ); //update "chain"

		//if ( player.state != "idle" ) { Debug.Log ( player.state ); }
		//TODO: charging states, tracking charge % and previous charge percent
		//TODO: feedback on charge breakpoints being reached.
		if ( player.state == "xnormal" )
		{
			//do sector based on time in state + direction
			float angle = 90.0f * GetComponent<CustomController>().facing;
			float damage = xNormalBaseDamage * player.offense;
			AttackSystem.hitSector ( new Vector2( transform.position.x, transform.position.y ), xNormalAngle * -0.5f + angle, xNormalAngle * 0.5f + angle, 2.0f, damage * dt, player.id );
		}
		if ( player.state == "xsmash" )
		{
			//do sector based on time in state + direction
			//change sector angle based on frame: 1/20 s = 1/15 rotation (24 degrees)
			spinTime += dt;
			float minAngle = ( ( ( ( 180.0f - spinTime * 360.0f / (0.75f / 2.0f) ) % 360.0f) + 360.0f) % 360.0f);
			float maxAngle = ( ( ( ( minAngle + 90.0f ) % 360.0f) + 360.0f) % 360.0f);
			AttackSystem.hitSector ( new Vector2( transform.position.x, transform.position.y ), minAngle, maxAngle, 2.0f, attackDamage * dt, player.id );
			//Sound?
			if ( spinTime > (0.75f / 2.0f) )
			{
				spinTime -= (0.75f / 2.0f);
				GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightSwoosh, 0.5f );
			}

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
			float damage = yNormalBaseDamage * player.offense;
			float angle = 90.0f * GetComponent<CustomController>().facing;
			float x = transform.position.x;
			float y = transform.position.y;
			float angleRad = angle * Mathf.Deg2Rad;
			float min = 0.25f; //minimum or base width & height of the hitbox
			float r = 1.5f;   //factor applied to cos / sin to extend hitbox
			//
			float xmin = Mathf.Min ( x - min, x - min + r * Mathf.Cos ( angleRad ) );
			float ymin = Mathf.Min ( y - min, y - min + r * Mathf.Sin ( angleRad ) );
			float xmax = Mathf.Max ( x + min, x + min + r * Mathf.Cos ( angleRad ) );
			float ymax = Mathf.Max ( y + min, y + min + r * Mathf.Sin ( angleRad ) );

			AttackSystem.hitBox ( new Rect( xmin, ymin, (xmax - xmin), (ymax - ymin) ), damage * dt, player.id );
		}
		if ( player.state == "ysmash" )
		{
			//charge until duration runs out / you hit something (other than another player?)
			float angle = GetComponent<CustomController>().facing * Mathf.PI / 2;
			float speed = 13.0f * dt;
			if ( ySmashHalted ) { speed = 0.0f; }
			float x = transform.position.x;
			float y = transform.position.y;
			float min = 0.5f; //minimum or base width & height of the hitbox
			float r = 1.25f;   //factor applied to cos / sin to extend hitbox
			//
			float xmin = Mathf.Min ( x - min, x - min + r * Mathf.Cos ( angle ) );
			float ymin = Mathf.Min ( y - min, y - min + r * Mathf.Sin ( angle ) );
			float xmax = Mathf.Max ( x + min, x + min + r * Mathf.Cos ( angle ) );
			float ymax = Mathf.Max ( y + min, y + min + r * Mathf.Sin ( angle ) );

			GetComponent<CustomController>().MoveNaoPlz( new Vector3( speed * Mathf.Cos ( angle ), speed * Mathf.Sin ( angle ), 0.0f ) );
			AttackSystem.hitBox ( new Rect( xmin, ymin, (xmax - xmin), (ymax - ymin) ), attackDamage * dt, player.id );
		}

		if ( player.state == "revcharge" )
		{
			#region sound
			if ( GetComponent<AudioSource>().isPlaying == false )
			{
				GetComponent<AudioSource>().volume = 1.0f;
				GetComponent<AudioSource>().clip = GetComponent<SoundStorage>().KnightRevLoop;
				GetComponent<AudioSource>().Play();
			}
			#endregion
			#region animation
			if ( GetComponent<CustomController>().move_vec.sqrMagnitude > 0.0f ) //moving
			{
				gameObject.GetComponent<Animator>().Play( "shuffle_" +  player.GetAniSuffix() );
			}
			else //idle
			{
				gameObject.GetComponent<Animator>().Play( "rev_" +  player.GetAniSuffix() );
			}
			#endregion

		}

		if ( player.state == "xcharge" || player.state == "ycharge" )
		{
			#region animation
			if ( GetComponent<CustomController>().move_vec.sqrMagnitude > 0.0f ) //moving
			{
				gameObject.GetComponent<Animator>().Play( "shuffle_" +  player.GetAniSuffix() );
			}
			else //idle
			{
				gameObject.GetComponent<Animator>().Play( "shuffle_idle_" +  player.GetAniSuffix() );
			}
			#endregion
		}

		if ( player.state == "idle" )
		{
			#region animation
			if ( ! player.isDowned )
			{
				if ( player.isCarrier == false )
				{
					GetComponent<Animator>().Play ( "idle_" +  player.GetAniSuffix() );
				}
				else
				{
					GetComponent<Animator>().Play ( "carry_idle_" +  player.GetAniSuffix() );
				}
			}
			else
			{
				GetComponent<Animator>().Play ( "downed_" +  player.GetAniSuffix() );
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

		xCharged = player.state == "xcharge" && xHoldTime >= xChargeMin;
		xCharged2 = player.state == "xcharge" && xHoldTime >= xChargeMax;
		yCharged = player.state == "ycharge" & yHoldTime >= yChargeMin;
	}

	public void OnHitCallback()
	{
		//If you hit an enemy, charge up your resource.
		GetComponent<VibrationManager>().AddVibrationForThisFrame( 0.0f, 0.35f);

		if ( ignoreOnHitCallback ) { return; }

		float deltaResource = 1.0f / 8.0f;
		player.resource = Mathf.Min ( player.resource + deltaResource, 1.0f );
		player.resourceGraceT = GraceT;

		//don't charge multiple times for a multihit move.
		ignoreOnHitCallback = true;

		//Halt y smash.
		ySmashHalted = true;
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
		//Can enqueue a parry out of recovery?
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" || player.state == "revcharge" )
		{
			ChangeState( "parry" );
		}
		if ( player.state == "xwinddown" || player.state == "xwinddown2" || 
		     player.state == "ywinddown" || player.state == "ywinddown2" )
		{
			//enqueue a parry
			player.nextState = "parry";
		}
	}
	public void BReleased()
	{
		//Called when B is released.
		//DO NOTHING.
		bHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
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
		if ( player.isDowned ) { return; }
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
			ChangeState( "xwindup" );
			if ( player.state == "revcharge" )
			{
				GetComponent<AudioSource>().Stop ();
			}
		}
		else if ( player.state == "xwinddown" || player.state == "ywinddown" ) //cut off frames if you attack during recovery
		{
			//use combo timing to shorten animation cycle between attacks.
			//pressing x or y while the attack / recovery is going -> queues up a 0 windup attack.
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.nextState = "xcharge"; //queue up.
		}
		else if ( player.state == "xwinddown2" || player.state == "ywinddown2" )
		{
			//you missed the recovery, but queue it up.
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.nextState = "xwindup"; //queue up.
			//state after this -> charge / normal
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
		if ( player.isDowned ) { return; }
		if ( player.state != "xwindup" && player.state != "xcharge" && player.state != "xwinddown") { return; }
		if ( player.state == "xwinddown" )
		{
			//released early, queue up normal slash
			player.nextState = "xnormal"; //queue up.
		}
		if ( player.state == "xwindup" )
		{
			//released early, queue up normal slash
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			player.nextState = "xnormal"; //queue up.
		}
		if ( player.state == "xcharge" )
		{
			if ( xHoldTime < xChargeMin )
			{
				//Horizontal attack
				ChangeState( "xnormal" );
				player.speedMultiplier = player.speedMultiplier * 2.0f;
			}
			else
			{
				//Spin2Win
				ChangeState( "xsmash" );
				player.speedMultiplier = player.speedMultiplier * 2.0f * 0.75f;
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
				this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 0.66f ), 0.25f );
				//TODO: sound
				GetComponent<VibrationManager>().ScheduleVibration ( 0.25f, 0.25f, 0.25f );
			}
			if ( xHoldTime - dt < xChargeMax && xHoldTime >= xChargeMax )
			{
				//feedback: max charge reached.
				this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 0.66f ), 0.25f );
				//TODO: sound
				GetComponent<VibrationManager>().ScheduleVibration ( 0.5f, 0.5f, 0.25f );
			}
		}
	}
	public void XRest( float dt )
	{
		//Called every frame X is in its natural state.
		//Solves a wierd enqueue state problem.
		if ( player.state == "xcharge" )
		{
			ChangeState( "xnormal" );
		}
	}
	#endregion
	#region Y
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
		//Initialize overhead strike.
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" || player.state == "revcharge" )
		{
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			ChangeState( "ywindup" );
			if ( player.state == "revcharge" )
			{
				GetComponent<AudioSource>().Stop ();
			}
		}
		else if ( player.state == "xwinddown" || player.state == "ywinddown" ) //cut off frames if you attack during recovery
		{
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.nextState = "ycharge"; //queue up.
		}
		else if ( player.state == "xwinddown2" || player.state == "ywinddown2" )
		{
			//you missed the recovery, but queue it up.
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.nextState = "ywindup"; //queue up
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		//Do overhead strike / blastoff
		//Animation + state vars
		//poll gamestate, do hitbox hit detection.
		//with the charge, hitbox detection over time. A bit more complex?
		if ( player.isDowned ) { return; }
		if ( player.state != "ywindup" && player.state != "ycharge" ) { return; }
		if ( yHoldTime < yChargeMin )
		{
			//overhead strike
			player.nextState = "ynormal"; //queue up
		}
		else
		{
			//Blast Off
			ChangeState ( "ysmash" );
		}
		player.speedMultiplier = player.speedMultiplier * 2.0f;
		yHoldTime = 0.0f;
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
		//Charge overhead strike -> blastoff
		yHoldTime += dt;
		if ( player.state == "ycharge" )
		{
			if ( yHoldTime - dt < yChargeMin && yHoldTime >= yChargeMin )
			{
				this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 0.66f ), 0.25f );
				//TODO: sound
				GetComponent<VibrationManager>().ScheduleVibration ( 0.25f, 0.25f, 0.25f );
			}
			if ( yHoldTime - dt < yChargeMax && yHoldTime >= yChargeMax )
			{
				this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 0.66f ), 0.25f );
				//TODO: sound
				GetComponent<VibrationManager>().ScheduleVibration ( 0.5f, 0.5f, 0.25f );
			}
		}
	}
	public void YRest( float dt )
	{
		//Called every frame Y is in its natural state.
		if ( player.state == "ycharge" )
		{
			ChangeState( "ynormal" );
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
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "revcharge" );
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightRevLoop, 1.0f );
		}
	}
	public void RTReleased()
	{
		//Called when RT is released.
		//Set up no degen window.
		//Reset speed.
		if ( player.isDowned ) { return; }
		if ( player.state == "revcharge" )
		{
			ChangeState ( "idle" );
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			GetComponent<AudioSource>().Stop ();
		}
		rtHoldTime = 0.0f;
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
		//Increase rev charge slightly?
		if ( player.state == "revcharge" )
		{
			//1.0f * dt = 100% per second
			float deltaResource = ( 0.5f - 3.0f * Mathf.Cos ( rtHoldTime * Mathf.PI * 2.0f ) ) * 0.20f * dt; //0.10f * dt;

			player.resource = Mathf.Min ( player.resource + deltaResource, 1.0f );
			player.resourceGraceT = 0.5f;

			float vibrationL = 0.05f + 0.05f * (0.5f * ( Mathf.Sin( rtHoldTime * Mathf.PI * 2.0f ) + 1.0f ) );
			float vibrationR = 0.1f + 0.1f * (0.5f * ( Mathf.Cos( rtHoldTime * Mathf.PI * 2.0f ) + 1.0f ) );
			GetComponent<VibrationManager>().AddVibrationForThisFrame ( vibrationL, vibrationR );
		}
		rtHoldTime += dt;
	}
	#endregion

	private void UpdateResource( float dt )
	{
		if ( player.state == "idle" || player.state == "walk" )
		{
			//degen grace timer
			player.resourceGraceT -= dt;
		}
		
		if ( player.resourceGraceT <= 0.0f )
		{
			player.resource = Mathf.Max ( player.resource - 1.0f * dt, 0.0f );
		}
	}

	void ChangeState( string newState )
	{
		//Forces the state to change next frame.
		player.nextState = newState;
		player.stateTimer = 0.0f;
	}

	void OnStateChange( string newState )
	{
		//handles setting up chains of state changes.
		#region X
		//windup -> charge -> attack -> winddown -> winddown2 -> idle
		//                                       -> charge ...
		//windup -> charge -> smash  -> winddown -> winddown2 -> idle
		//                                       -> charge ...
		if ( newState == "xwindup" )
		{
			player.nextState = "xcharge";
			player.stateTimer = 0.05f * 7.0f; //7 frame windup
			player.interruptHP = xChargeInterruptHP;
			GetComponent<Animator>().Play ( "xslash_" +  player.GetAniSuffix() + "_windup" );
		}
		else if ( newState == "xcharge" )
		{
			player.nextState = "xcharge"; //freeze at this state
			player.interruptHP = xChargeInterruptHP;
		}
		else if ( newState == "xnormal" )
		{
			//initialize the attack.
			player.canMove = false;
			player.stateTimer = 0.05f * 5.0f;
			player.nextState = "xwinddown";//"idle";
			player.canMove = false;
			ignoreOnHitCallback = false;
			//player.resource = Mathf.Min ( player.resource + 1.0f / 8.0f, 1.0f );
			//player.resourceGraceT = xNormalGraceT;
			player.interruptHP = xNormalInterruptHP;
			#region animation
			gameObject.GetComponent<Animator>().Play( "xslash_" + player.GetAniSuffix() );
			#endregion
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightSwoosh, 1.0f );
		}
		else if ( newState == "xwinddown" )
		{
			player.nextState = "xwinddown2";
			player.stateTimer = 0.05f * 5.0f; //5 frame recovery

			if ( xSmashUsed == true )
			{
				player.speedMultiplier = player.speedMultiplier / 0.75f; //TODO: move this to a float.
				xSmashUsed = false;
			}
		}
		else if ( newState == "xwinddown2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 6.0f; //6 frame recovery
		}
		else if ( newState == "xsmash" )
		{
			player.canMove = true;
			player.nextState = "xwinddown";
			player.stateTimer = 0.05f * 15.0f; //15 frame attack (+ can be extended)

			//get power based on charge and resource.
			float chargePercent = Mathf.Min ( (xHoldTime - xChargeMin), (xChargeMax - xChargeMin) ) / (xChargeMax - xChargeMin); //0.0f - 1.0f
			float chainPercent = player.resource * player.resource; //0.0f - 1.0f, non-linear scaling
			//multiplicative stacking?
			float baseDamage = (xSmashBaseDamage + (xSmashAddDamage * chargePercent) );
			float multiplier = player.offense * (1.0f + xSmashChainBonus * chainPercent);
			attackDamage = baseDamage * multiplier;
			player.resource = 0.0f;
			spinTime = 0.0f;
			spinToWinExtensions = 0;
			player.interruptHP = xSmashInterruptHP; //uninterruptable, for all intents and purposes.
			GetComponent<Animator>().Play( "hurricane_spin" );
			maxSpinToWinExtensions = 1 + ( (int) (2.0f * chargePercent) ); //Scale with charge
			//Mathf.Min ( xHoldTime, xChargeMax ) / xChargeMax;
			ignoreOnHitCallback = false;
			xSmashUsed = true;
		}
		#endregion
		#region y
		//windup -> charge -> attack -> winddown -> winddown2 -> idle
		//                                       -> charge ...
		//windup -> charge -> smash  -> winddown -> winddown2 -> idle
		//                                       -> charge ...
		else if ( newState == "ywindup" )
		{
			player.nextState = "ycharge";
			player.stateTimer = 0.05f * 7.0f; //7 frame windup
			player.interruptHP = yChargeInterruptHP;
			GetComponent<Animator>().Play ( "xslash_" +  player.GetAniSuffix() + "_windup" );
		}
		else if ( newState == "ycharge" )
		{
			player.nextState = "ycharge"; //freeze the state in a loop.
			player.interruptHP = yChargeInterruptHP;
		}
		else if ( newState == "ynormal" )
		{
			//initialize the attack
			player.canMove = false;
			player.stateTimer = 0.05f * 12.0f; //10 frame attack (6 animated, rest pause)
			player.nextState = "ywinddown";
			ignoreOnHitCallback = false;
			//player.resource = Mathf.Min ( player.resource + 1.0f / 8.0f, 1.0f );
			//player.resourceGraceT = yNormalGraceT;
			player.interruptHP = yNormalInterruptHP;
			GetComponent<Animator>().Play( "yslash_" + player.GetAniSuffix() );
			//TODO: box to box collision detection
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightSlice, 1.0f );
		}
		else if ( newState == "ywinddown" )
		{
			player.nextState = "ywinddown2";
			player.stateTimer = 0.05f * 4.0f; //4 frame recovery
			GetComponent<Animator>().Play ( "yslash_" +  player.GetAniSuffix() + "_winddown" );
		}
		else if ( newState == "ywinddown2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frame recovery
			GetComponent<Animator>().Play ( "xslash_" +  player.GetAniSuffix() + "_winddown2" );
		}
		else if ( newState == "ysmash" )
		{
			//initialize the attack
			player.canMove = false;
			player.nextState = "ywinddown";
			player.interruptHP = ySmashInterruptHP;
			ySmashHalted = false;

			//get power based on charge and resource.
			float chargePercent = Mathf.Min ( (yHoldTime - yChargeMin), (yChargeMax - yChargeMin) ) / (yChargeMax - yChargeMin); //0.0f - 1.0f
			float chainPercent = player.resource * player.resource; //0.0f - 1.0f, non-linear scaling
			//multiplicative stacking?
			float baseDamage = (ySmashBaseDamage + (ySmashAddDamage * chargePercent) );
			float multiplier = player.offense * (1.0f + ySmashChainBonus * chainPercent);
			attackDamage = baseDamage * multiplier;
			player.stateTimer = 1.0f + 1.0f * chargePercent;
			player.resource = 0;
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightBlastOff, 1.0f );
			//TODO: hitbox to hitbox collision detection.
			GetComponent<Animator>().Play( "blastoff_" + player.GetAniSuffix() );
			ignoreOnHitCallback = false;
		}
		#endregion
		else if ( newState == "revcharge" )
		{
			player.nextState = "revcharge"; //freeze state in a loop
			player.stateTimer = 0.0f;
		}
		else if ( newState == "walk" )
		{
			#region animation
			if ( ! player.isDowned )
			{
				gameObject.GetComponent<Animator>().Play( "walk_" + player.GetAniSuffix() );
			}
			else
			{
				gameObject.GetComponent<Animator>().Play( "crawl_" + player.GetAniSuffix() );
			}
			#endregion
		}
		#region parry
		else if ( newState == "parry" )
		{
			player.nextState = "idle";
			player.stateTimer = parryTime;
			player.canMove = false;
			player.isParrying = true;
			player.parryTimer = parryTime;
			#region animation
			gameObject.GetComponent<Animator>().Play( "parry_" +  player.GetAniSuffix() );
			#endregion
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().MonkStoneSkinOn, 1.0f );
		}
		#endregion
		else if ( newState == "idle" )
		{
			//for interruption
			/*
			bHoldTime = 0.0f;
			xHoldTime = 0.0f;
			yHoldTime = 0.0f;
			rtHoldTime = 0.0f;
			*/
			player.speedMultiplier = 1.0f; //redundant.
		}
	}
}
