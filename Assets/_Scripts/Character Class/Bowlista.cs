using UnityEngine;
using System.Collections;

public class Bowlista : MonoBehaviour, ClassFunctionalityInterface 
{
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	private Vector3 prevPos;
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
		#region ELSEWHERE
		/*
		Vector3 pos = this.gameObject.transform.position; //aliasing
		if ( pos.x == prevPos.x && pos.y == prevPos.y )
		{
			player.resource = Mathf.Min ( player.resource + 0.05f, 1.0f );
		}
		else
		{
			player.resource = Mathf.Max ( player.resource - 0.05f, 0.0f );
		}
		//update
		prevPos = new Vector3( pos.x, pos.y, pos.z );
		*/
		#endregion

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
		//TODO: dodge roll
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
	}
}
