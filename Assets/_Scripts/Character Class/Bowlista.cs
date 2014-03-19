using UnityEngine;
using System.Collections;

public class Bowlista : MonoBehaviour, ClassFunctionalityInterface 
{
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";
	private Vector3 prevPos;

	private const float focusRegenRate = 0.5f; // % of max resource / s
	private const float focusDegenRate = 1.0f; // % of max resource / s

	#region move data
	//button hold times
	private float percentCharge = 0.0f;

	#region RT
	private const float rtBaseDamage = 10.0f; //Gust: base damage (0 charge)
	private const float rtAddDamage = 10.0f;  //Gust: additional damage (100% charge)
	private const float rtStartAngle = 90.0f; //Gust: starting aim angle (goes to 0)
	private const float rtStartRange = 2.0f;  //Gust: range ( @   0% charge )
	private const float rtEndRange = 15.0f;   //Gust: range ( @ 100% charge )
	//TODO: make the (damage * area) be ~ constant?

	private float rtHoldTime = 0.0f;
	private const float rtChargeMax = 0.5f;        //Maximum hold time: more than this confers no benefit.
	private const float rtChargeMultiplier = 1.0f; //aim speed scaling factor (with focus) (1.0f = +100% charge speed at max focus)
    #endregion

	#region X
	private const float blowbackRadius = 1.0f; //Blowback: radius
	#endregion

	#region Dodge
	private const float dodgeTime = 0.05f * 5.0f; //5 frames
	private const float dodgeSpeed = 8.0f;        //dodge speed (units / s)
	private Vector3 dodgeVec = new Vector3();     //dodge vector
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
		UpdateResource ( dt );

		if ( player.state != "idle" ) { Debug.Log ( player.state ); } //DEBUG

		//custom state logic
		if ( player.state == "dodge" )
		{
			//move them along dodge vector
			Vector3 vec = new Vector3( dodgeVec.x * dt, dodgeVec.y * dt, 0.0f );
			controller.MoveNaoPlz ( vec );
		}

		if ( player.state == "rtcharge" )
		{
			float scalingFactor = Mathf.Min ( rtHoldTime, rtChargeMax ) / rtChargeMax; //t scale

			float r = Mathf.Lerp ( rtStartRange, rtEndRange, scalingFactor ); //range (radius of sector)

			float baseAngle = controller.aimAngle;
			float incrAngle = Mathf.Lerp ( rtStartAngle / 2.0f, 0.0f, scalingFactor );
			float lowAngle = Mathf.Deg2Rad * (baseAngle - incrAngle);
			float highAngle = Mathf.Deg2Rad * (baseAngle + incrAngle);
			float x = transform.position.x;
			float y = transform.position.y;
			Debug.DrawLine ( transform.position, new Vector3( x + r * Mathf.Cos( lowAngle ), y + r * Mathf.Sin ( lowAngle ), 0.0f ), 
			                 new Color( 0.0f, 1.0f, 0.0f, 1.0f ) );
			Debug.DrawLine ( transform.position, new Vector3( x + r * Mathf.Cos( highAngle ), y + r * Mathf.Sin ( highAngle ), 0.0f ), 
			                 new Color( 0.0f, 1.0f, 0.0f, 1.0f ) );
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
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "dodge" ); //not doge: http://en.wikipedia.org/wiki/Doge_%28meme%29
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
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			player.nextState = "xcharge";
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		if ( player.isDowned ) { return; }
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
		if ( player.isDowned ) { return; }
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
		//TODO: charge
		if ( player.isDowned ) { return; }
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
		percentCharge = Mathf.Min( rtHoldTime, rtChargeMax ) / rtChargeMax;
		rtHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
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
			//increase hold time by an amount increased by focus.
			rtHoldTime = Mathf.Min ( rtHoldTime + (1.0f + (rtChargeMultiplier * player.resource) ) * dt, rtChargeMax );
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
			player.resource = Mathf.Min( player.resource + focusRegenRate * dt, 1.0f );
		}
		else
		{
			player.resource = Mathf.Max ( player.resource - focusDegenRate * dt, 0.0f );
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
			//attack cone / line, based on charge.
			//float percentCharge = ( Mathf.Min ( rtHoldTime, rtChargeMax ) / rtChargeMax ); //made class-wide
			float baseDamage = rtBaseDamage + ( rtAddDamage * percentCharge );
			float damage = baseDamage * player.offense;
			float range = Mathf.Lerp ( rtStartRange, rtEndRange, percentCharge );

			float baseAngle = controller.aimAngle;
			float x = transform.position.x;
			float y = transform.position.y;
			//sector / ray
			if ( rtHoldTime < rtChargeMax )
			{
				//sector
				float incrAngle = Mathf.Lerp ( rtStartAngle / 2.0f, 0.0f, percentCharge );
				float minAngle = Mathf.Deg2Rad * (baseAngle - incrAngle);
				float maxAngle = Mathf.Deg2Rad * (baseAngle + incrAngle);
				AttackSystem.hitSector ( new Vector2( x, y ), minAngle, maxAngle, range, damage, player.id );
			}
			else
			{
				//ray (piercing ray?)
				Vector2 end = new Vector2( x + range * Mathf.Cos ( Mathf.Deg2Rad * baseAngle ),
				                           y + range * Mathf.Sin ( Mathf.Deg2Rad * baseAngle ) );
				AttackSystem.hitLineSegment( new Vector2( x, y ), end, damage, player.id );
			}
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
			for ( int i = 0; i < GameState.players.Length; i++ )
			{
				float dist = 0.0f;
				if ( dist <= blowbackRadius)
				{
					//TODO: push out of PBAOE
				}
			}
		}
		else if ( newState == "xwinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.0f;
			//player.stateTimer = 0.05f * 1.0f; //1 frame
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
