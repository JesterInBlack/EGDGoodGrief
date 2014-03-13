using UnityEngine;
using System.Collections;

public class Bowlista : MonoBehaviour, ClassFunctionalityInterface 
{
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";
	private Vector3 prevPos;

	private const float dodgeTime = 0.05f * 5.0f; //5 frames
	private const float dodgeSpeed = 8.0f;        //dodge speed (units / s)
	private Vector3 dodgeVec = new Vector3();     //dodge vector
	//button hold times
	private float rtHoldTime = 0.0f;
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
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "dodge" ); //not doge: http://en.wikipedia.org/wiki/Doge_%28meme%29
		}
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
		if ( player.state == "idle" || player.state == "walk" )
		{
			player.nextState = "xcharge";
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		if ( player.state != "xcharge" ) { return; }
		player.nextState = "blowback";
		player.stateTimer = 0.0f;
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
		//TODO: charge
		if ( player.state == "idle" || player.state == "walk" )
		{
			player.state = "rtcharge";
			player.stateTimer = 0.0f;
			player.nextState = "rtcharge";
		}
	}
	public void RTReleased()
	{
		//Called when RT is released.
		if ( player.state != "rtcharge" ) { return; }
		//TODO: fire
		player.nextState = "rtfire";
		player.stateTimer = 0.0f;
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
		if ( player.state == "rtcharge" )
		{
			//charge += dt;
		}
	}
	#endregion

	private void UpdateResource( float dt )
	{
		//Gain focus while standing still.
		//TODO: EXCEPTION FOR KNOCKBACK?
		//TODO: Grace period? (doesn't degen instantly?)
		Vector3 pos = this.gameObject.transform.position; //aliasing
		if ( pos.x == prevPos.x && pos.y == prevPos.y )
		{
			player.resource = Mathf.Min( player.resource + 0.5f * dt, 1.0f );
		}
		else
		{
			player.resource = Mathf.Max ( player.resource - 1.0f * dt, 0.0f );
		}
		prevPos = this.gameObject.transform.position;
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
		#region firing
		//(no windup) charge -> fire -> winddown
		if ( newState == "rtcharge" )
		{
			player.nextState = "rtcharge"; //freeze the state in a loop
			player.stateTimer = 0.0f;
			player.speedMultiplier = player.speedMultiplier * 0.5f;
		}
		else if ( newState == "rtfire" )
		{
			player.canMove = false;
			player.stateTimer = 0.05f * 1.0f; //1 frame
			player.nextState = "rtwinddown";
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			//TODO: attack cone / line, based on charge. Then, void charge.
		}
		else if ( newState == "rtwinddown" )
		{
			player.canMove = true;
			player.stateTimer = 0.05f * 1.0f; //1 frame
			player.nextState = "idle";
		}
		#endregion
		#region blowback
		//(no windup) charge -> fire -> winddown
		else if ( newState == "xcharge" )
		{
			player.nextState = "xcharge"; //freeze the state in a loop
			player.stateTimer = 0.0f;
			player.speedMultiplier = player.speedMultiplier * 0.5f;
		}
		else if ( newState == "blowback" )
		{
			player.nextState = "xwinddown";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			player.speedMultiplier = player.speedMultiplier * 2.0f;
			//TODO: push out of PBAoE.
		}
		else if ( newState == "xwinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		#endregion
		#region dodge
		else if ( newState == "dodge" )
		{
			player.nextState = "idle";
			player.stateTimer = dodgeTime;
			player.canMove = false;
			player.isDodging = true;
			player.dodgeTimer = dodgeTime;
			//Get dodge vector (move vector's direction, or facing if idle).
			dodgeVec = controller.move_vec.normalized * dodgeSpeed;
			//if idle, default to forewards
			if ( dodgeVec.sqrMagnitude == 0.0f )
			{
				float angle = controller.facing * Mathf.PI / 2.0f;
				dodgeVec = new Vector3( Mathf.Cos ( angle ), Mathf.Sin ( angle ), 0.0f ) * dodgeSpeed;
			}
			#region animation
			#endregion
		}
		#endregion
	}
}
