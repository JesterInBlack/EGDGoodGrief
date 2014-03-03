using UnityEngine;
using System.Collections;

public class StoneFist : MonoBehaviour, ClassFunctionalityInterface 
{

	#region vars
	private Player player;
	private CustomController controller;
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
		if ( player.state == "ycharge" )
		{
			//TODO: take damage, take sediment
			//TODO: if damage limit is exceeded, break, counterattack.
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
		//TODO: stone skin (parry)
		//Charge up: 1 hit defensive shield / duration, breaks on hit
		//if another player breaks the 1 hit shield, they get way knocked back.
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
			ChangeState ( "xwindup" );
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		//TODO: sand laser
		if ( player.state != "xcharge" && player.state != "xwindup" ) { return; }
		if ( player.state == "xcharge" )
		{
			//TODO: hitbox
		}
		ChangeState ( "xwinddown" );
	}
	public void XHeld( float dt )
	{
		//Called every frame X is held down.
		//TODO: charge sand laser
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
		//TODO: monodirectional shield
		if ( player.state == "idle" || player.state == "walk" )
		{
			//TODO: change direction not allowed, move is?
			ChangeState( "ycharge" );
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		//TODO: counterattack
		if ( player.state != "ycharge" ) { return; }
		ChangeState ( "ywinddown" );
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
		//TODO: charge up counterattack
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
		//TODO: sandstorm
		if ( player.state != "idle" ) { return; }
		ChangeState ( "rtwindup" );
	}
	public void RTReleased()
	{
		//Called when RT is released.
		if ( player.state != "rtwindup" && player.state != "sandstorm" ) { return; }
		ChangeState( "rtwinddown" );
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
		if ( player.state != "sandstorm" ) { return; }
		//TODO: charge?
		//TODO: DoT
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
		#region x
		if ( player.state == "xwindup" )
		{
			player.nextState = "xcharge";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		else if ( player.state == "xcharge" )
		{
			player.nextState = "xwinddown"; //freeze in a loop
			player.stateTimer = 0.0f; 
		}
		else if ( player.state == "xwinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		#endregion
		#region y
		else if ( player.state == "ycharge" )
		{
			player.nextState = "ycharge"; //freeze in a loop
			player.stateTimer = 0.0f;
		}
		else if ( player.state == "ywinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		#endregion
		#region rt
		else if ( player.state == "rtwindup" )
		{
			player.nextState = "sandstorm";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		else if ( player.state == "sandstorm" )
		{
			player.nextState = "sandstorm"; //freeze in a loop
			player.stateTimer = 0.0f;
		}
		else if ( player.state == "rtwinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
		}
		#endregion
	}
}
