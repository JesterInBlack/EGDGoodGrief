using UnityEngine;
using System.Collections;

public class StoneFist : MonoBehaviour, ClassFunctionalityInterface 
{

	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	#region move data
	//Button Hold Times
	#region X
	private const float xBaseDamage = 1.0f; //Sand Blast: base damage (0% charge)
	private const float xAddDamage = 1.0f;  //Sand Blast: additional damage (100% charge)

	private float xHoldTime  = 0.0f;
	private const float xChargeMax = 10.0f;    //maximum hold time: more than this confers no benefit.
	#endregion

	#region Y
	private float yHoldTime  = 0.0f;
	private float yDamage = 0.0f; //damage the shield has taken
	private const float yBaseDamage = 1.0f; //base damage of the shield attack
	private const float yAddDamage = 1.0f;  //additional damage of the shield attack, based on the % damage / sediment it took.

	private const float shieldHP = 1.0f;       //if the shield takes this much damage, it breaks.
	private const float shieldDegenRate = 1.0f / 3.0f;  //the % of sediment the shield drains, per second.
	#endregion

	#region B
	private float bHoldTime  = 0.0f;
	private const float bChargeTime = 1.0f;    //hold time to use the B buff.
	#endregion
	
	#region RT
	private float rtHoldTime = 0.0f;
	private const float rtDamage = 1.0f;  //Sandstorm DPS
	private const float rtRadius = 0.67f; //Sandstorm area
	private const float rtResourceRate = 0.1f; // % resource gained per second while sandstorm is up
	#endregion

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
		UpdateResource ( dt ); //update "sediment accumulation"

		if ( player.state != "idle" ) { Debug.Log ( player.state ); } //DEBUG

		//Custom State Logic
		if ( player.state == "ycharge" )
		{
			//take sediment
			player.resource = Mathf.Max ( player.resource - shieldDegenRate * dt, 0.0f );
			if ( player.resource == 0 )
			{
				ChangeState ( "ywinddown" );
			}
			//if damage limit is exceeded, break, counterattack.
			if ( yDamage >= shieldHP )
			{
				ChangeState( "ywinddown" );
			}
		}
		if ( player.state == "sandstorm" )
		{
			float x = transform.position.x;
			float y = transform.position.y;
			AttackSystem.hitCircle ( new Vector2( x, y ), rtRadius, rtDamage * dt, player.id );
			player.resource = Mathf.Min ( player.resource + rtResourceRate * dt, 1.0f );
			//TODO: suck enemies / other players in (SLOWLY)
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
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "bcharge" );
		}
	}
	public void BReleased()
	{
		//Called when B is released.
		if ( player.state != "bcharge" ) { return; }
		if ( bHoldTime >= bChargeTime )
		{
			//TODO: 1 hit shield w/ duration buff?
			//set a stoneskin flag
			//put it on a timer
			ChangeState ( "idle" );
		}
		else
		{
			//not fully charged.
			//TODO: play fail sound?
			ChangeState ( "idle" );
		}
		bHoldTime = 0.0f;
	}
	public void BHeld( float dt )
	{
		bHoldTime += dt;
		//Called every frame B is held down.
		if ( player.state != "bcharge" ) { return; }
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
		xHoldTime = 0.0f;
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
		xHoldTime += dt;
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
		yHoldTime = 0.0f;
		if ( player.state != "ycharge" ) { return; }
		ChangeState ( "ywinddown" );
	}
	public void YHeld( float dt )
	{
		//Called every frame Y is held down.
		//TODO: charge up counterattack
		yHoldTime += dt;
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
		rtHoldTime = 0.0f;
		if ( player.state != "rtwindup" && player.state != "sandstorm" ) { return; }
		ChangeState( "rtwinddown" );
	}
	public void RTHeld( float dt )
	{
		//Called every frame RT is held down.
		rtHoldTime += dt;
		if ( player.state != "sandstorm" ) { return; }
		//TODO: charge?
		//TODO: DoT
	}
	#endregion

	private void UpdateResource( float dt )
	{
		//Constantly gain sediment accumulation.
		//Some moves degenerate it.
		player.resource = Mathf.Min ( player.resource + 0.25f * dt, 1.0f );
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
			yDamage = 0.0f; //reset damage
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
