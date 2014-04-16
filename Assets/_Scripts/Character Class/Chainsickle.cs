using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public class Chainsickle : MonoBehaviour, ClassFunctionalityInterface 
{
	//TODO: hitboxes for moves, interrupt HPs
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	private bool isSpinning = false; //whether or not the ninja is spinning his chain
	private const float spinInterruptHP = 1.0f;

	private bool thumbstickNeutral = false;

	#region move data

	#region X
	private const float xMeleeCombo1BaseDamage = 1.0f;   //Melee combo hit 1: base damage  (Single Hit)
	private const float xMeleeCombo2BaseDamage = 1.0f;   //Melee combo hit 2: base damage  (Single Hit)
	private const float xMeleeCombo3BaseDamage = 1.0f;   //Melee combo hit 3: base damage  (Single Hit)
	private const float xRangedCombo1BaseDamage = 1.0f;  //Ranged combo hit 1: base damage (Single Hit)
	private const float xRangedCombo2BaseDamage = 1.0f;  //Ranged combo hit 2: base damage (Single Hit)
	private const float xRangedCombo3BaseDamage = 1.0f;  //Ranged combo hit 3: base damage (Single Hit)

	private const float xMeleeCombo1InterruptHP = 1.0f;
	private const float xMeleeCombo2InterruptHP = 1.0f;
	private const float xMeleeCombo3InterruptHP = 1.0f;
	private const float xRangedCombo1InterruptHP = 1.0f;
	private const float xRangedCombo2InterruptHP = 1.0f;
	private const float xRangedCombo3InterruptHP = 1.0f;
	#endregion

	#region Y
	private const float yMeleeBaseDamage = 1.0f;         //Melee smash: base damage (Single Hit)
	private const float yRangedBaseDamage = 1.0f;        //Melee smash: base damage (Single Hit)

	private const float yMeleeInterruptHP = 1.0f;
	private const float yRangedInterruptHP = 1.0f;
	#endregion

	private const float hookBaseDamage = 1.0f;           //Hook: base damage (Single Hit)
	private const float hookInterruptHP = 1.0f;          //?
	#endregion

	#region Dodge
	private const float dodgeTime = 0.05f * 5.0f; //5 frames
	private const float dodgeSpeed = 8.0f;        //dodge speed (units / s)
	private Vector3 dodgeVec = new Vector3();     //dodge vector
	#endregion

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
		UpdateResource ( dt );

		thumbstickNeutral = ( Mathf.Abs( GamePad.GetState( controller.playerIndex ).ThumbSticks.Right.X ) + 
			Mathf.Abs ( GamePad.GetState( controller.playerIndex ).ThumbSticks.Right.Y ) )
			<= 0.1f;

		if ( player.state != "idle" ) { Debug.Log ( player.state ); } //DEBUG

		//custom state logic
		if ( player.state == "flip" )
		{
			//move them along dodge vector
			Vector3 vec = new Vector3( dodgeVec.x * dt, dodgeVec.y * dt, 0.0f );
			controller.MoveNaoPlz ( vec );
		}

		//trap state changes
		if ( prevState != player.state )
		{
			OnStateChange( player.state );
		}
		prevState = player.state;
	}

	public void OnHitCallback() {}

	#region B
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
		//TODO: flip (dodge roll)
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "flip" );
		}
	}
	public void BReleased()
	{
		//Called when B is released.
		if ( player.isDowned ) { return; }
	}
	public void BHeld( float dt )
	{
		//Called every frame B is held down.
	}
	#endregion
	
	#region X
	//Tap / Hold
	public void XPressed()
	{
		//Called when X is pressed.
		//combo 1 -> combo 2 -> combo 3 -> ...
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || 
		     player.state == "xranged1" || player.state == "xranged2" || player.state == "xranged3" ||
		     player.state == "xnormal1" || player.state == "xnormal2" || player.state == "xnormal3" )
		{
			if ( isSpinning )
			{
				#region ranged
				if ( player.state == "idle" )
				{
					ChangeState ( "xranged1" );
				}
				else if ( player.state == "xranged1" )
				{
					player.nextState = "xranged2"; //queue up.
				}
				else if ( player.state == "xranged2" )
				{
					player.nextState = "xranged3"; //queue up.
				}
				else if ( player.state == "xranged3" )
				{
					player.nextState = "xranged1"; //queue up.
				}
				#endregion
			}
			else
			{
				#region melee
				if ( player.state == "idle" )
				{
					ChangeState ( "xnormal1" );
				}
				else if ( player.state == "xnormal1" )
				{
					player.nextState = "xnormal2"; //queue up.
				}
				else if ( player.state == "xnormal2" )
				{
					player.nextState = "xnormal3"; //queue up.
				}
				else if ( player.state == "xnormal3" )
				{
					player.nextState = "xnormal1"; //queue up.
				}
				#endregion
			}
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		if ( player.isDowned ) { return; }
	}
	public void XHeld( float dt )
	{
		//Called every frame X is held down.
	}
	public void XRest( float dt )
	{
		//Called every frame X is in it's natural state.
	}
	#endregion
	
	#region Y
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
		//TODO: read spin flag
		//ball smash / ball throw
		if ( player.isDowned ) { return; }
		if ( isSpinning )
		{
			//ranged
			ChangeState ( "yranged" );
		}
		else
		{
			//normal
			ChangeState ( "ynormal" );
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		if ( player.isDowned ) { return; }
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
	}
	public void YRest( float dt )
	{
		//Called every frame Y is in it's natural state.
	}
	#endregion
	
	#region RT
	//Tap / Hold
	public void RTPressed()
	{
		//Called when RT is pressed.
		//TODO: set spin flag
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "spinup" );
		}
	}
	public void RTReleased()
	{
		//Called when RT is released.
		//TODO: unset spin flag
		//TODO: if twin stick not neutral, and not another button pressed, hook.
		//TODO: the longer you spin, the further you go?
		if ( player.isDowned ) { return; }
		if ( isSpinning)
		{
			if ( player.state == "spin" )
			{
				if ( ! thumbstickNeutral )
				{
					//TODO: hook
					//isSpinning = false;
				}
				else
				{
					ChangeState ( "spindown" );
				}
			}
			else if ( player.state == "spinup" )
			{
				//enqueue hook / spindown
				if ( ! thumbstickNeutral )
				{
					//TODO: hook
					//isSpinning = false;
				}
				else
				{
					player.nextState = "spindown"; //queue up.
				}
			}
		}
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
	}
	#endregion

	private void UpdateResource( float dt )
	{
		//Gain style by doing stuff.
		//Lose style for not doing anything for a while.
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
		#region x
		//ranged
		if ( newState == "xranged1" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xRangedCombo1InterruptHP;
			#region hitbox
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			Vector2 pos = new Vector2( x, y );
			float angle = controller.facing * Mathf.PI / 2.0f;
			float sweep = Mathf.Deg2Rad * 45.0f;
			float r = 2.0f;
			float damage = xRangedCombo1BaseDamage;
			AttackSystem.hitSector ( pos, angle - sweep / 2.0f, angle + sweep / 2.0f, r, damage, player.id  );
			//TODO: ice effects?
			#endregion
		}
		else if ( newState == "xranged2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xRangedCombo2InterruptHP;
		}
		else if ( newState == "xranged3" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xRangedCombo3InterruptHP;
		}
		//normal
		else if ( newState == "xnormal1" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xMeleeCombo1InterruptHP;
		}
		else if ( newState == "xnormal2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xMeleeCombo2InterruptHP;
		}
		else if ( newState == "xnormal3" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
			player.interruptHP = xMeleeCombo3InterruptHP;
		}
		#endregion
		#region y
		//ranged
		else if ( newState == "yranged" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			player.interruptHP = yRangedInterruptHP;
		}
		//normal
		else if ( newState == "ynormal" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			player.interruptHP = yMeleeInterruptHP;
		}
		#endregion
		#region spin
		else if ( newState == "spinup" )
		{
			player.nextState = "spin";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			isSpinning = true;
			player.speedMultiplier = player.speedMultiplier * 0.5f;
			player.interruptHP = spinInterruptHP;
		}
		else if ( newState == "spin" )
		{
			player.nextState = "spin";
			player.stateTimer = 0.0f;
		}
		else if ( newState == "spindown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			isSpinning = false;
			player.speedMultiplier = player.speedMultiplier * 2.0f;
		}
		#endregion
		#region flip
		else if ( newState == "flip" )
		{
			player.nextState = "idle";
			player.stateTimer = dodgeTime;
			player.canMove = false;
			player.isDodging = true;
			player.dodgeTimer = dodgeTime;
			//Get dodge vector (move vector's direction, or facing if idle).
			dodgeVec = controller.move_vec.normalized * dodgeSpeed;
			//if idle, default to backwards
			if ( dodgeVec.sqrMagnitude == 0.0f )
			{
				float angle = ( (controller.facing + 2) % 4) * Mathf.PI / 2.0f;
				dodgeVec = new Vector3( Mathf.Cos ( angle ), Mathf.Sin ( angle ), 0.0f ) * dodgeSpeed;
			}
			#region animation
			#endregion
		}
		#endregion
	}
}
