using UnityEngine;
using System.Collections;

public class StoneFist : MonoBehaviour, ClassFunctionalityInterface 
{
	//TODO: y shield
	//TODO: interrupt HPs.
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	#region move data
	//Button Hold Times
	#region X
	private const float xBaseDamage = 1.0f;  //Sand Blast: base damage (0% charge)
	private const float xAddDamage = 1.0f;   //Sand Blast: additional damage (100% charge)

	private const float xInterruptHP = 1.0f; //Sand Blast: interrupt damage threshold.

	private float xHoldTime  = 0.0f;
	private const float xChargeMax = 2.0f;   //maximum hold time: more than this confers no benefit.
	#endregion

	#region Y
	private float yHoldTime  = 0.0f;
	private const float yBaseDamage = 1.0f;  //base damage of the shield attack
	private const float yAddDamage = 1.0f;   //additional damage of the shield attack, based on the % damage / sediment it took.

	//this one is uninterruptable. (how to integrate with the interrupt core player call?)
	private const float shieldMaxHP = 1.0f;             //if the shield takes this much damage, it breaks.
	private float shieldHP = 1.0f;                      //the shield's HP
	private const float shieldDegenRate = 1.0f / 3.0f;  //the % of sediment the shield drains, per second.
	#endregion

	#region B
	private float bHoldTime  = 0.0f;
	private const float bChargeTime = 1.0f;    //hold time to use the B buff.
	#endregion
	
	#region RT
	private float rtHoldTime = 0.0f;
	private const float rtDamage = 2.0f;  //Sandstorm DPS
	private const float rtRadius = 1.0f;  //Sandstorm area
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
			shieldHP -= shieldMaxHP * shieldDegenRate * dt; //TODO: separate shield HP and amount of sediment consumed tracker.
			if ( player.resource == 0 )
			{
				ChangeState ( "ywinddown" );
			}
			//if damage limit is exceeded, break, counterattack.
			if ( shieldHP <= 0.0f )
			{
				ChangeState( "ywinddown" );
			}
		}
		if ( player.state == "sandstorm" )
		{
			float x = transform.position.x;
			float y = transform.position.y;
			float damage = player.offense * (rtDamage * dt);
			AttackSystem.hitCircle ( new Vector2( x, y ), rtRadius, damage, player.id );
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

	public void OnHitCallback() {}

	#region B
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
		//TODO: stone skin (parry)
		//Charge up: 1 hit defensive shield / duration, breaks on hit
		//if another player breaks the 1 hit shield, they get way knocked back.
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			ChangeState ( "bcharge" );
		}
	}
	public void BReleased()
	{
		//Called when B is released.
		if ( player.isDowned ) { return; }
		if ( player.state != "bcharge" ) { return; }
		if ( bHoldTime >= bChargeTime )
		{
			//TODO: 1 hit shield w/ duration buff?
			//set a stoneskin flag
			player.isStoneSkin = true;
			//put it on a timer
			player.stoneSkinTimer = 15.0f;
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkStoneSkinOn, 1.0f );
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
		if ( bHoldTime >= bChargeTime && bHoldTime - dt < bChargeTime )
		{
			this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 1.0f, 0.66f), 0.25f );
		}

		//automatically trigger stoneskin if fully charged.
		if ( bHoldTime >= bChargeTime )
		{
			//1 hit shield w/ duration buff?
			//set a stoneskin flag
			player.isStoneSkin = true;
			//put it on a timer
			player.stoneSkinTimer = 15.0f;
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkStoneSkinOn, 1.0f );
			ChangeState ( "idle" );
			bHoldTime = 0.0f;
		}
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
			ChangeState ( "xwindup" );
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		//TODO: sand laser
		float savedHoldTime = xHoldTime;
		xHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
		if ( player.state != "xcharge" && player.state != "xwindup" ) { return; }
		if ( player.state == "xcharge" )
		{
			#region hitbox
			//damage
			float percentCharge = Mathf.Min ( savedHoldTime, xChargeMax ) / xChargeMax;
			float damage = player.offense * (xBaseDamage + xAddDamage * percentCharge);
			//hitbox
			float angle = controller.facing * Mathf.PI / 2.0f;
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float w = 5.0f * Mathf.Cos ( angle ) + (0.5f + 1.0f * percentCharge ) * Mathf.Cos ( angle + Mathf.PI / 2.0f );
			float h = 5.0f * Mathf.Sin ( angle ) + (0.5f + 1.0f * percentCharge ) * Mathf.Sin ( angle + Mathf.PI / 2.0f );

			//shift so the box is centered on the player: 
			//move along the perpindicular axis to the target
			if ( controller.facing == 1 || controller.facing == 3 )
			{
				x -= w / 2.0f;
			}
			else
			{
				y -= h / 2.0f;
			}

			//fix negatives (swap x1 <-> x2 and y1 <-> y2)
			if ( w < 0.0f ) 
			{
				x = x + w;
				w = Mathf.Abs ( w );
			}
			if ( h < 0.0f )
			{
				y = y + h;
				h = Mathf.Abs ( h );
			}

			AttackSystem.hitBox ( new Rect( x, y, w, h ), damage, player.id  );
			#endregion
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkPunch, 1.0f );
		}
		ChangeState ( "xwinddown" );
	}
	public void XHeld( float dt )
	{
		//Called every frame X is held down.
		//TODO: charge sand laser
		xHoldTime += dt;
		if ( player.state != "xcharge" ) { return; }
		if ( xHoldTime >= xChargeMax && xHoldTime - dt < xChargeMax )
		{
			this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 1.0f, 0.66f), 0.25f );
		}
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
		if ( player.isDowned ) { return; }
		if ( player.state == "idle" || player.state == "walk" )
		{
			//TODO: change direction not allowed, move is?
			//TODO?: have a start-up cost / wind up window? (so you can't just spam block just in time?)
			ChangeState( "ycharge" );
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkRockRaise, 1.0f );
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		yHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
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
		if ( player.isDowned ) { return; }
		if ( player.state != "idle" ) { return; }
		ChangeState ( "rtwindup" );
	}
	public void RTReleased()
	{
		//Called when RT is released.
		rtHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
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

    public void OnHitCallback( int attackerID, float damage )
	{
		if ( player.state == "ycharge" )
		{
			//take hp from shield.
			float finalDamage = damage / player.defense;
			shieldHP -= finalDamage;
		}
	}

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
			GetComponent<Animator>().Play ( "punch_charge_" + player.GetAniSuffix() );
		}
		else if ( player.state == "xcharge" )
		{
			player.nextState = "xcharge"; //freeze in a loop
			player.stateTimer = 0.0f; 
		}
		else if ( player.state == "xwinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			GetComponent<Animator>().Play ( "punch_" + player.GetAniSuffix() );
		}
		#endregion
		#region y
		else if ( player.state == "ycharge" )
		{
			player.nextState = "ycharge"; //freeze in a loop
			player.stateTimer = 0.0f;
			shieldHP = shieldMaxHP; //reset damage
			GetComponent<Animator>().Play ( "guard_" + player.GetAniSuffix() );
		}
		else if ( player.state == "ywinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 1.0f; //1 frame
			//attack
			#region hitbox
			//hitbox
			float angle = controller.facing * Mathf.PI / 2.0f;
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float w = 2.0f * Mathf.Cos ( angle ) + 1.5f * Mathf.Cos ( angle + Mathf.PI / 2.0f );
			float h = 2.0f * Mathf.Sin ( angle ) + 1.5f * Mathf.Sin ( angle + Mathf.PI / 2.0f );
			
			//shift so the box is centered on the player: 
			//move along the perpindicular axis to the target
			if ( controller.facing == 1 || controller.facing == 3 )
			{
				x -= w / 2.0f;
			}
			else
			{
				y -= h / 2.0f;
			}
			
			//fix negatives (swap x1 <-> x2 and y1 <-> y2)
			if ( w < 0.0f ) 
			{
				x = x + w;
				w = Mathf.Abs ( w );
			}
			if ( h < 0.0f )
			{
				y = y + h;
				h = Mathf.Abs ( h );
			}
			float percentCharge = 1.0f - Mathf.Max ( 0.0f, ( shieldHP / shieldMaxHP ) );
			float damage = player.offense * (yBaseDamage + yAddDamage * percentCharge);
			
			AttackSystem.hitBox ( new Rect( x, y, w, h ), damage, player.id  );
			#endregion
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkRockBreak, 1.0f );
			GetComponent<Animator>().Play ( "counter_" + player.GetAniSuffix() );
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
		else if ( player.state == "bcharge" )
		{
			GetComponent<Animator>().Play ( "stoneskin_" + player.GetAniSuffix() );
		}
	}
}
