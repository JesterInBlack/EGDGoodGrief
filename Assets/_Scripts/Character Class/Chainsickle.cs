using UnityEngine;
using System.Collections;

public class Chainsickle : MonoBehaviour, ClassFunctionalityInterface 
{
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	private const float dodgeTime = 0.05f * 5.0f; //5 frames
	private const float dodgeSpeed = 8.0f;        //dodge speed (units / s)
	private Vector3 dodgeVec = new Vector3();     //dodge vector

	private bool isSpinning = false; //whether or not the ninja is spinning his chain
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

		if ( player.state != "idle" ) { Debug.Log ( player.state ); } //DEBUG

		//custom state logic
		if ( player.state == "dodge" )
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

	#region B
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
		//TODO: flip (dodge roll)
	}
	public void BReleased()
	{
		//Called when B is released.
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
		if ( isSpinning)
		{
			if ( player.state == "spin" )
			{
				if ( true )
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
				if ( true )
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
		}
		else if ( newState == "xranged2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
		}
		else if ( newState == "xranged3" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
		}
		//normal
		else if ( newState == "xnormal1" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
		}
		else if ( newState == "xnormal2" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
		}
		else if ( newState == "xnormal3" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 5.0f; //5 frames
		}
		#endregion
		#region y
		//ranged
		else if ( newState == "yranged" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		//normal
		else if ( newState == "ynormal" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		#endregion
		#region spin
		else if ( newState == "spinup" )
		{
			player.nextState = "spin";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			isSpinning = true;
			player.speedMultiplier = player.speedMultiplier * 0.5f;
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
