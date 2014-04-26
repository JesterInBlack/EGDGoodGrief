using UnityEngine;
using System.Collections;

public class StoneFist : MonoBehaviour, ClassFunctionalityInterface 
{
	//TODO: interrupt HPs.
	#region vars
	private Player player;
	private CustomController controller;
	private string prevState = "";

	#region move data
	//Button Hold Times
	#region X
	private const float xBaseDamage = 75.0f;   //Sand Blast: base damage (0% charge)
	private const float xAddDamage = 150.0f;   //Sand Blast: additional damage (100% charge)

	private const float xInterruptHP = 100.0f; //Sand Blast: interrupt damage threshold.

	private float xHoldTime  = 0.0f;
	private const float xChargeMin = 0.5f;   //minimum charge time to get ranged fist.
	private const float xChargeMax = 1.5f;   //maximum hold time: more than this confers no benefit.

	public bool xCharged = false;
	#endregion

	#region Y
	private float yHoldTime  = 0.0f;
	private const float yBaseDamage = 50.0f;  //base damage of the shield attack
	private const float yAddDamage = 450.0f;   //additional damage of the shield attack, based on the % damage / sediment it took.

	//this one is uninterruptable. (how to integrate with the interrupt core player call?)
	private const float shieldMaxHP = 70.0f;             //if the shield takes this much damage, it breaks.
	private float shieldHP = 70.0f;                      //the shield's HP
	private const float shieldDegenRate = 1.0f / 10.0f;    //the % of sediment the shield drains, per second.
	#endregion

	#region B
	private float bHoldTime  = 0.0f;
	private const float bChargeTime = 0.5f;    //hold time to use the B buff.
	#endregion
	
	#region RT
	private float rtHoldTime = 0.0f;
	private const float rtDamage = 20.0f;  //Sandstorm DPS
	private const float rtRadius = 1.2f;   //Sandstorm area
	private const float rtResourceRate = 0.4f; // % resource gained per second while sandstorm is up
	#endregion

	#endregion
	private const float passiveResourceRate = 0.25f; //what % of resource is regenerated per second.
	float savedHoldTime = 0.0f;

	public GameObject sandFistPrefab; //for instantiating the animated effect
	public GameObject shieldPrefab;   //for instantiating the stone wall
	private GameObject myShield;      //reference to the currently active shield. (send signal to destroy)
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

		//if ( player.state != "idle" ) { Debug.Log ( player.state ); } //DEBUG

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
		else
		{
			//not holding y.
			if ( myShield != null )
			{
				myShield.GetComponent<StoneWall>().Shatter();
			}
		}

		if ( player.state == "xcharge" )
		{
			if ( xHoldTime - dt < xChargeMin && xHoldTime >= xChargeMin )
			{
				//feedback: charge reached.
				this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 0.66f ), 0.25f );
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.PlayerCharge, 1.0f );
				GetComponent<VibrationManager>().ScheduleVibration ( 0.25f, 0.25f, 0.25f );
			}
		}
		if ( player.state == "sandstorm" )
		{
			float x = transform.position.x;
			float y = transform.position.y;
			float damage = player.offense * (rtDamage * dt);
			AttackSystem.hitCircle ( new Vector2( x, y ), rtRadius, damage, player.id );
			player.resource = Mathf.Min ( player.resource + rtResourceRate * dt, 1.0f );
			//suck enemies / other players in (SLOWLY)
			AttackSystem.Suck ( new Vector2( transform.position.x, transform.position.y ), 0.1f, dt );
		}

		//trap state changes
		if ( prevState != player.state )
		{
			OnStateChange( player.state );
		}
		prevState = player.state;

		#region Animation
		if ( player.state == "sandstorm" )
		{
			#region sound
			if ( GetComponent<AudioSource>().isPlaying == false )
			{
				GetComponent<AudioSource>().volume = 1.0f;
				GetComponent<AudioSource>().clip = SoundStorage.MonkSandStorm;
				if ( ! GetComponent<AudioSource>().isPlaying )
				{
					GetComponent<AudioSource>().Play();
				}
			}
			#endregion
			#region animation
			if ( GetComponent<CustomController>().move_vec.sqrMagnitude > 0.0f ) //moving
			{
				gameObject.GetComponent<Animator>().Play( "walk_" +  player.GetAniSuffix() );
			}
			else //idle
			{
				gameObject.GetComponent<Animator>().Play( "idle_" +  player.GetAniSuffix() );
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
		#endregion

		if ( player.state == "xcharge" && (xHoldTime >= xChargeMin || savedHoldTime >= xChargeMin) )
		{
			xCharged = true;
		}
		else if ( player.state == "xwinddown" )
		{
			//retain charge
		}
		else
		{
			xCharged = false;
		}
	}

	public void OnHitCallback() 
	{
		//If you hit an enemy, basically nothing happens.
		GetComponent<VibrationManager>().AddVibrationForThisFrame( 0.0f, 0.35f);
	}

	public void OnWasHit( int attackerID, float damage )
	{
		//whoever wrote the original version of this ... >:O
		if ( player.state == "ycharge" )
		{
			//take hp from shield.
			/*
			if ( attackerID != -1 )
			{
				damage = damage * 0.025f; //immensely reduce damage from players.
			}
			*/
			float finalDamage = damage / player.defense;
			shieldHP -= finalDamage;
		}
	}

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
		bHoldTime = 0.0f;
	}
	public void BReleased()
	{
		//Called when B is released.
		if ( player.isDowned ) { return; }
		if ( player.state != "bcharge" ) { return; }
		//changed to automatically activate.
		if ( bHoldTime >= bChargeTime )
		{
			//TODO: 1 hit shield w/ duration buff?
			//set a stoneskin flag
			player.isStoneSkin = true;
			//put it on a timer
			player.stoneSkinTimer = 5.0f;
			//GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkStoneSkinOn, 1.0f );
			ChangeState ( "bwinddown" );
		}
		else if ( bHoldTime < bChargeTime )
		{
			//not fully charged.
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuCancel, 1.0f );
			ChangeState ( "idle" );
		}
		bHoldTime = 0.0f;
	}
	public void BHeld( float dt )
	{
		bHoldTime += dt;
		//Called every frame B is held down.
		if ( player.state != "bcharge" ) { return; }
		//automatically trigger stoneskin if fully charged.
		if ( bHoldTime >= bChargeTime && bHoldTime - dt < bChargeTime )
		{
			this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 1.0f, 0.66f), 0.25f );
			//1 hit shield w/ duration buff?
			//set a stoneskin flag
			player.isStoneSkin = true;
			//put it on a timer
			player.stoneSkinTimer = 7.5f;
			//GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkStoneSkinOn, 1.0f );
			ChangeState ( "bwinddown" );
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
		if ( player.state == "xwinddown" )
		{
			player.nextState = "xwindup";
		}
	}
	public void XReleased()
	{
		//Called when X is released.
		//TODO: sand laser
		savedHoldTime = xHoldTime;
		xHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
		if ( player.state != "xcharge" && player.state != "xwindup" ) { return; }
		if ( player.state == "xcharge" )
		{
			ChangeState ( "xwinddown" );
		}
		else if ( player.state == "xwindup" )
		{
			player.nextState = "xwinddown"; //queue up.
		}
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
			if ( player.resource >= 0.50f )
			{
				//TODO: change direction not allowed, move is?
				//TODO?: have a start-up cost / wind up window? (so you can't just spam block just in time?)
				ChangeState( "ywindup" );
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkRockRaise, 1.0f );
			}
			else
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuCancel, 1.0f );
			}
		}
		else if ( player.state == "xwinddown" || player.state == "ywinddown" )
		{
			//added leniency with input when you're going for the shield.
			player.nextState = "ywindup";
		}
	}
	public void YReleased()
	{
		//Called when Y is released.
		yHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
		if ( player.nextState == "ywindup" ) { player.nextState = "idle"; } //reset queued action on release.
		if ( player.state != "ycharge" && player.state != "ywindup" ) { return; }
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
		if ( player.state != "idle" && player.state != "walk" ) { return; }
		ChangeState ( "rtwindup" );
	}
	public void RTReleased()
	{
		//Called when RT is released.
		rtHoldTime = 0.0f;
		if ( player.isDowned ) { return; }
		if ( player.state != "rtwindup" && player.state != "sandstorm" ) { return; }
		ChangeState( "rtwinddown" );
		GetComponent<AudioSource>().Stop();
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
		if ( player.state != "ycharge" && player.state != "ywindup" )
		{
			player.resource = Mathf.Min ( player.resource + passiveResourceRate * dt, 1.0f );
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
		if ( player.state == "xwindup" )
		{
			player.nextState = "xcharge";
			player.stateTimer = 0.05f * 7.0f * 0.67f; //7 frames @ 30FPS
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
			player.stateTimer = 0.05f * 10.0f * 0.33f; //10 frames @ 60FPS
			GetComponent<Animator>().Play ( "punch_" + player.GetAniSuffix() );
			#region hitbox
			//damage
			float percentCharge = Mathf.Max ( 0.0f, (Mathf.Min ( savedHoldTime, xChargeMax ) - xChargeMin) / (xChargeMax - xChargeMin) );
			float damage = player.offense * (xBaseDamage + xAddDamage * percentCharge);
			//hitbox
			float angle = controller.facing * Mathf.PI / 2.0f;
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float w = 0.5f * Mathf.Cos ( angle ) + 1.0f * Mathf.Cos ( angle + Mathf.PI / 2.0f );
			float h = 0.5f * Mathf.Sin ( angle ) + 1.0f * Mathf.Sin ( angle + Mathf.PI / 2.0f );
			if ( percentCharge > 0.0f && player.resource >= 0.25f )
			{
				//= length formula (along axis)                         + width formula (perpindicular to axis)
				w = (2.5f + 2.5f * percentCharge) * Mathf.Cos ( angle ) + Mathf.Cos ( angle + Mathf.PI / 2.0f );
				h = (2.5f + 2.5f * percentCharge) * Mathf.Sin ( angle ) + Mathf.Sin ( angle + Mathf.PI / 2.0f );
				//make long-range sand fist.
				GameObject obj = (GameObject)Instantiate ( sandFistPrefab, transform.position, Quaternion.identity );
				obj.GetComponent<StonePunch>().range = Mathf.Max ( Mathf.Abs( w ), Mathf.Abs ( h ) );
				obj.GetComponent<StonePunch>().angle = angle;
				player.resource = Mathf.Max ( 0.0f, player.resource - 0.25f );
			}
			
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
			#region PlayRandomSound
			float rn = Random.Range ( 0.0f, 100.0f );
			float possibilities = 4;
			if ( rn <= 100.0f / possibilities * 1.0f )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkPunch, 1.0f );
			}
			else if ( rn <= 100.0f / possibilities * 2.0f )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkPunch2, 1.0f );
			}
			else if ( rn <= 100.0f / possibilities * 3.0f )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkPunch3, 1.0f );
			}
			else if ( rn <= 100.0f / possibilities * 4.0f )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkPunch4, 1.0f );
			}
			#endregion
		}
		#endregion
		#region y
		else if ( player.state == "ywindup" )
		{
			player.nextState = "ycharge";     //transition to immunity state!
			player.stateTimer = 0.05f * 3.0f; //3 frames
			player.canMove = false;
			GetComponent<Animator>().Play ( "guard_" + player.GetAniSuffix() );
		}
		else if ( player.state == "ycharge" )
		{
			player.nextState = "ycharge"; //freeze in a loop
			player.stateTimer = 0.0f;
			player.canMove = false;
			shieldHP = shieldMaxHP; //reset damage
			//Spawn wall
			GameObject obj = (GameObject)Instantiate( shieldPrefab, transform.position, Quaternion.identity );
			obj.GetComponent<StoneWall>().facing = controller.facing;
			obj.transform.position += new Vector3( 0.5f * Mathf.Cos ( Mathf.PI / 2.0f * controller.facing ), 
			                                       0.5f * Mathf.Sin ( Mathf.PI / 2.0f * controller.facing ), 0.0f );
			if ( myShield != null ) { myShield.GetComponent<StoneWall>().Shatter(); }
			myShield = obj;
		}
		else if ( player.state == "ywinddown" )
		{
			player.nextState = "idle";
			player.stateTimer = 0.05f * 6.0f; //6 frames
			player.resource = Mathf.Max ( 0.0f, player.resource - 0.50f ); //hefty cost to break early to stop spam 4 damage.
			player.canMove = true;
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
			if ( myShield != null )
			{
				myShield.GetComponent<StoneWall>().Shatter();
			}
			myShield = null;
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
			player.stateTimer = 0.0f;
			player.nextState = "bcharge"; //freeze in loop
			GetComponent<Animator>().Play ( "stoneskin_" + player.GetAniSuffix() );
			player.canMove = false;
		}
		else if ( player.state == "bwinddown" )
		{
			player.stateTimer = 0.05f * 5.0f;
			player.nextState = "idle";
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MonkStoneSkinOn, 1.0f );
		}
	}
}
