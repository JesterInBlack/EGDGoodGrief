using UnityEngine;
using System.Collections;

public enum CharacterClasses { KNIGHT, ARCHER, NINJA, DEFENDER };

public class Player : MonoBehaviour 
{
	//TODO: fix throw ani for item use.
	//TODO: add anti-cooperation for phermononing, vampire fanging other players
	//TODO: add cooperation for aura-ing up if your aura is still active / it buffs other players.

	#region vars
	[HideInInspector]
	public int id; //used for identity checks.

	public float HP;
	[HideInInspector]
	public float maxHP;
	[HideInInspector]
	public float baseMaxHP = 100.0f;

	public ScorePasser scorePasser; //loaded on awake
	public float score = 0.0f;
	public float glory = 0.0f;
	public ParticleSystem gloryParticles;

	public float defense = 1.0f;  //defensive power: (2x = 1/2 damage). Base: 1 = 1x damage.
	public float offense = 1.0f;  //offensive power: (2x = 2x  damage). Base: 1 = 1x damage.

	public CharacterClasses characterclass = CharacterClasses.KNIGHT; //enum

	//state data?
	//idle, walk, <custom via plugin>
	public string state = "idle";     //current state
	[HideInInspector]
	public float stateTimer = 0.0f;   //state timer
	[HideInInspector]
	public string nextState = "idle"; //next state: on state timer reaching 0, transition occurs.
	[HideInInspector]
	public string prevState = "idle"; //previous state

	public bool canMove = true;           //disable moving while attacking?
	[HideInInspector]
	public float speedMultiplier = 1.0f;  //able to move, but at a slower pace? Volatile, constantly set + reset.
	[HideInInspector]
	public float speedMultiplier2 = 1.0f; //non-volatile speed multiplier
	//bool overrideMoveAni;

	[HideInInspector]
	public bool isDodging = false;   //currently dodging?
	[HideInInspector]
	public float dodgeTimer = 0.0f;  //time left in dodge animation?

	[HideInInspector]
	public bool isParrying = false;  //currently parrying?
	[HideInInspector]
	public float parryTimer = 0.0f;  //time left in parry animation?

	[HideInInspector]
	public bool isStoneSkin = false; //currently stone skin buffed? (bad practice: bubble up)
	[HideInInspector]
	public float stoneSkinTimer = 0.0f; //time left in stone skin buff.

	//Downed / Carrying State Stuff
	public bool isDowned = false;    //if you're downed
	public bool isCarrier = false;   //if you're carrying another player.
	public bool isCarried = false;   //if you're being carried by another player.
	[HideInInspector]
	public GameObject Carried;       //ref to the player you're carrying.
	[HideInInspector]
	public GameObject Carrier;       //ref to the player carrying you.
	[HideInInspector]
	public Vector2 carryVec;         //unit vector representing your carry direction.
	
	private Vector2 knockbackVec;

	//interruption
	public float interruptHP = 0.0f;   //"interrupt hp": if this reaches 0, you get interrupted. Set by moves.
	private float interruptDR = 0.0f;  //interrupt diminishing returns factor. 
	                                   //  (makes you harder to interrupt the more you get interrupted)

	//Buff / Debuff data?
	public bool isInBulletTime = false;     //Immune to the stop watch slow?
	[HideInInspector]
	public float bulletTimeDuration = 0.0f; //How long (real time) until time control immunity expires.

	public HeadGrabber headGrabber;         //set in inspector.

	public ArrayList buffs = new ArrayList();

	//items x 3
	//item cooldowns?
	public Item[] items = new Item[3];
	public int itemIndex = 0;
	const int ITEM_SLOT_COUNT = 3;

	//unique mechanic data
	public float resource = 0.0f;        //chain / focus / style / sediment. Ranges from 0 to 1
	[HideInInspector]
	public float resourceGraceT = 0.0f;  //at 0, resource begins degeneration
	//private Vector3 prevPos;           //previous position, for detecting movement. (focus degen) //removed because unused.
	#endregion

	//Use this for pre-initialization
	void Awake ()
	{
		id = (int)GetComponent<CustomController>().playerIndex;
		GameObject tempObj = GameObject.Find( "MenuDataSaver" );
		MenuDataSaver temp = null;
		if ( tempObj != null )
		{
			temp = tempObj.GetComponent<MenuDataSaver>();
		}

		#region default
		if ( temp == null ) //Fix errors from skipping the main menu.
		{
			characterclass = CharacterClasses.KNIGHT; //default
			//read items from menu data.
			for ( int i = 0; i < 3; i++ )
			{
				items[i] = new Item();
				items[i].Construct ( ItemName.PHEROMONE_JAR ); //default
			}
		}
		#endregion

		else
		{
			//TODO: if this player is disconnected, remove them.
			/*
			if ( ! temp.playersConnected[ id ] ) 
			{ 
				Destroy( this.gameObject );
				return; 
			}
			*/

			//read class from menu data
			characterclass = temp.playerClasses[ id ];

			//read items from menu data.
			for ( int i = 0; i < 3; i++ )
			{
				items[i] = new Item();
				items[i].Construct ( temp.playerItems[ id, i ] );
			}

			//Set read flag (to delete the menu data to prevent clashing + buildup)
			temp.read[ id ] = true;
		}
		scorePasser = GameObject.Find ( "P" + (id + 1) + "ScoreData" ).GetComponent<ScorePasser>();
	}

	// Use this for initialization
	void Start () 
	{
		//initialize stats
		//GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f ); //Change this to alter colors per player

		score = 100.0f;

		maxHP = baseMaxHP;
		HP = maxHP;

		offense = 1.0f;
		defense = 1.0f;

		#region Class-Specific
		//Gets the class specific action handler for the controller.
		CustomController controller = this.gameObject.GetComponent<CustomController>();
		//Disable all the controller scripts
		this.gameObject.GetComponent<RocketSwordFunctions>().enabled = false;
		this.gameObject.GetComponent<Bowlista>().enabled = false;
		this.gameObject.GetComponent<StoneFist>().enabled = false;
		this.gameObject.GetComponent<Chainsickle>().enabled = false;
		//Set and re-enable the one controller script we're going to use.
		if ( characterclass == CharacterClasses.KNIGHT )
		{
			controller.actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent<RocketSwordFunctions>();
			this.gameObject.GetComponent<RocketSwordFunctions>().enabled = true;
			//TODO: set stats (speed, defense / hp)
		}
		else if ( characterclass == CharacterClasses.ARCHER )
		{
			controller.actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent<Bowlista>();
			this.gameObject.GetComponent<Bowlista>().enabled = true;
			//TODO: set stats (speed, defense / hp)
		}
		else if ( characterclass == CharacterClasses.DEFENDER )
		{
			controller.actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent<StoneFist>();
			this.gameObject.GetComponent<StoneFist>().enabled = true;
			//TODO: set stats (speed, defense / hp)
		}
		else if ( characterclass == CharacterClasses.NINJA )
		{
			controller.actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent<Chainsickle>();
			this.gameObject.GetComponent<Chainsickle>().enabled = true;
			//TODO: set stats (speed, defense / hp)
		}
		#endregion
	}
	
	// Update is called once per frame
	void Update () 
	{
		//isDowned = true; //DEBUG
		//HP -= Time.deltaTime * 1.0f; //DEBUG

		float t = Time.deltaTime;
		if ( ! isInBulletTime ) { t = t * StaticData.t_scale; }

		//Timer countdown
		#region timers
		stateTimer = Mathf.Max ( 0.0f, stateTimer - t );
		if ( stateTimer <= 0 ) 
		{ 
			//TODO: wrap this in a set state function
			//TODO: enum / const states.
			if ( nextState == "idle" )
			{
				canMove = true;
				speedMultiplier = 1.0f;
			}

			state = nextState; 
		}

		if ( isDodging ) 
		{ 
			dodgeTimer -= t;
			if ( dodgeTimer <= 0.0f )
			{
				isDodging = false;
			}
		}

		if ( isParrying )
		{
			parryTimer -= t;
			if ( parryTimer <= 0.0f )
			{
				isParrying = false;
			}
		}

		if ( isStoneSkin  )
		{
			stoneSkinTimer -= t;
			if ( stoneSkinTimer <= 0.0f )
			{
				isStoneSkin = false;
			}
		}

		if ( isInBulletTime )
		{
			bulletTimeDuration -= t;
			if ( bulletTimeDuration <= 0 )
			{
				isInBulletTime = false;
			}
		}

		if ( state == "knockback" )
		{
			this.gameObject.GetComponent<Animator>().Play ( "knocked_" + GetAniSuffix() );
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float z = this.gameObject.transform.position.z;
			transform.position = new Vector3( x + (knockbackVec.x * t), y + (knockbackVec.y * t), z );
		}

		//Update items
		for ( int i = 0; i < 3; i++ )
		{
			items[i].Update( t );
		}

		//Update buffs
		for ( int i = 0; i < buffs.Count; i++ )
		{
			( (Buff) buffs[i] ).Update( t );
		}
		//Remove buffs that are tagged for removal
		for ( int i = buffs.Count - 1; i >= 0; i-- )
		{
			if ( ( (Buff) buffs[i] ).taggedForRemoval )
			{
				( (Buff) buffs[i] ).End();
				//Debug.Log ( "Buff expired." );
				buffs.RemoveAt ( i );
			}
		}

		//Check if player died (from a lethal DoT)
		if ( HP <= 0.0f && ! isDowned )
		{
			Die();
		}

		//Handle glory particles
		if ( glory > 0.0f )
		{
			gloryParticles.enableEmission = true;
			//gloryParticles.Play ();
			if ( gloryParticles != null )
			{
				gloryParticles.emissionRate = 40.0f * (1.0f + glory / 2.0f) * (1.0f + glory / 2.0f);
				gloryParticles.startLifetime = 1.0f / (1.0f + glory / 2.0f);
				gloryParticles.startSpeed = -1.0f * (1.0f + glory / 2.0f);
			}
		}
		else
		{
			gloryParticles.enableEmission = false;
			//gloryParticles.Stop ();
		}
		#endregion

		//trap state changes
		if ( prevState != state )
		{
			OnStateChange( state );
		}
		prevState = state;

		//prevPos = this.gameObject.transform.position; //removed because unused.
	}

	void Respawn()
	{
		//state reset?
		maxHP = baseMaxHP;
		HP = maxHP;
	}

	public void ChangeItemIndex( int increment )
	{
		GetComponent<Tutorial>().swappedItem = true;
		//increment: +1 or -1
		//check to make sure we're not currently charging an item.
		if ( ! (state == "item windup" || 
		        state == "item charge" || 
		        state == "item aim ray" || 
		        state == "item aim point") )
		{
			itemIndex = Mod( itemIndex + increment, ITEM_SLOT_COUNT );
		}
	}

	int Mod(int x, int m) 
	{
		//Custom modulus operator
		//To fix the stupid % returns negative thing in C#
		return (x % m + m) % m;
	}

	public void Hurt( float damage )
	{
		if ( isDowned ) { return; }
		if ( isParrying ) 
		{
			ScoreManager.AvoidedDamage ( id, damage );
			GetComponent<VibrationManager>().ScheduleVibration ( 0.35f, 0.35f, 0.10f );
			return; 
		}
		if ( isStoneSkin ) 
		{ 
			ScoreManager.AvoidedDamage ( id, damage );
			GetComponent<VibrationManager>().ScheduleVibration ( 0.35f, 0.35f, 0.10f );
			isStoneSkin = false; 
			return; 
		}
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			ScoreManager.AvoidedDamage ( id, damage );
			this.gameObject.GetComponent<StoneFist>().OnHitCallback( -1, damage );
			GetComponent<VibrationManager>().ScheduleVibration ( 0.20f, 0.20f, 0.10f );
			return; 
		}

		float vibration = Mathf.Min( 0.67f, (damage / maxHP) * 0.67f );
		GetComponent<VibrationManager>().ScheduleVibration ( vibration, vibration, 0.10f );

		//deal damage
		float finalDamage = damage / defense;
		ScoreManager.TookDamage ( id, finalDamage ); //reduce point loss by buffing defense.
		//sedimentary, dear watson.
		if ( characterclass == CharacterClasses.DEFENDER )
		{
			//TODO: make this sane.
			resource = 0.0f;
			finalDamage = damage - 1.0f;
		}
		HP -= finalDamage;
		if ( HP <= 0.0f )
		{
			Die();
		}

		//ASSUMPTION: hurt is only called by the boss' attacks.
		GameState.angerAxis = Mathf.Max ( 0.0f,  GameState.angerAxis - 0.05f );
		GameState.playerThreats[id] = Mathf.Max ( 0.0f,  GameState.playerThreats[id] - 10.0f );

		#region resource deduction
		//resource deduction. (chain, sediment)
		if ( characterclass == CharacterClasses.KNIGHT )
		{
			resource = 0.0f;
		}
		if ( characterclass == CharacterClasses.DEFENDER )
		{
			resource = 0.0f; //?
		}
		#endregion
	}

	public void Wound( float damage )
	{
		//TODO: figure out the conditons this will be called on.
		//consider hurt 1st impacting the parry / shield vars.
		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//reduce max hp
		maxHP -= damage;
		if ( maxHP <= 1.0f )
		{
			//having negative max HP makes less than no sense.
			maxHP = 1.0f;
		}
	}

	public void Interrupt( int attackerId, float damage )
	{
		//Attempt to interrupt the current move.
		//(Friendly player attacks you)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }
		if ( isStoneSkin ) 
		{
			GetComponent<VibrationManager>().ScheduleVibration ( 0.35f, 0.35f, 0.10f );
			if ( attackerId != -1 )
			{
				float x = this.gameObject.transform.position.x;
				float y = this.gameObject.transform.position.y;
				GameState.playerStates[ attackerId ].KnockBack ( 2.0f, new Vector2( x, y ) );
			}
			isStoneSkin = false;
			return; 
		}
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			GetComponent<VibrationManager>().ScheduleVibration ( 0.20f, 0.20f, 0.10f );
			this.gameObject.GetComponent<StoneFist>().OnHitCallback( attackerId, damage );
			return; 
		}

		//Flash red.
		this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 0.75f, 0.75f), 0.05f );
		GetComponent<VibrationManager>().ScheduleVibration ( 0.35f, 0.35f, 0.10f );

		//TODO: diminishing returns, thresholds for moves, move interrupt power scaling
		interruptHP -= damage;
		interruptDR++;

		//TODO: scale anti-cooperation based on how oftern interruption occurs
		//TODO: greatly improve this horrible naiieve implementation
		if ( attackerId != -1 )
		{
			GameState.cooperationAxis = Mathf.Max ( -1.0f,  GameState.cooperationAxis - 0.0025f );
		}

		if ( interruptHP <= 0.0f )
		{
			canMove = true;
			//TODO: play interrupt sound

			//TESTING: knockback on interrupt
			if ( attackerId != -1 )
			{
				float x = GameState.players[attackerId].transform.position.x;
				float y = GameState.players[attackerId].transform.position.y;
				KnockBack ( 2.0f, new Vector2( x, y ) );
			}

			//knockbackVec = Vector2.zero;
			state = "knockback";
			stateTimer = 0.5f;
			nextState = "idle";
			this.gameObject.GetComponent<Animator>().Play ( "knocked_" + GetAniSuffix() );
		}

		//TODO: Add a threshold before auras blacklist you
		//Go through the player's buffs, and remove any you have given them
		if ( attackerId != -1 ) 
		{
			Player aggressor = GameState.players[attackerId].GetComponent<Player>();
			aggressor.RemoveBuffsGivenByPlayer( id );
		}
	}

	public void KnockBack( float magnitude, Vector2 pos )
	{
		//Knock the player back? (based on where the attack comes from)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }
		if ( isStoneSkin ) { return; } //TODO: stone skin
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			//this.gameObject.GetComponent<StoneFist>().OnHitCallback( damage );
			return; 
		}

		//TODO: move knockback power scaling
		//TODO: effects that minimize / reduce knockback. (blocking, parrying, dodging?)

		knockbackVec = (new Vector2( transform.position.x, transform.position.y ) - pos ).normalized * magnitude;
		canMove = false;
		state = "knockback";
		stateTimer = 0.5f;
		nextState = "idle";

		this.gameObject.GetComponent<Animator>().Play ( "knocked_" + GetAniSuffix() );
	}

	public void Poison( float duration, float degen )
	{
		Buff myBuff = new Buff();
		myBuff.player = this;
		myBuff.giverId = -1;
		myBuff.blacklist = false;
		myBuff.regen = -1.0f * degen;
		myBuff.duration = duration;
		
		buffs.Add ( myBuff );
		myBuff.Start ();
	}
	
	public void Slow( float duration, float percent )
	{
		Buff myBuff = new Buff();
		myBuff.player = this;
		myBuff.giverId = -1;
		myBuff.blacklist = false;
		myBuff.speed = -1.0f * percent;
		myBuff.duration = duration;

		//remove any other slows.
		for ( int j = buffs.Count - 1; j >= 0; j-- )
		{
			if ( ( (Buff) buffs[j] ).isTheSameAs( myBuff ) )
			{
				( (Buff) buffs[j] ).End();
				buffs.RemoveAt ( j );
			}
		}
		buffs.Add ( myBuff );
		myBuff.Start ();
	}

	private void Die()
	{
		//deadz.
		//Go into downed state.
		isDowned = true;
		//interrupt any attacks or actions. Force state change.
		state = "idle";
		stateTimer = 0.0f;
		nextState = "idle";
		//deduct points
		ScoreManager.Death ( id );
		#region remove buffs
		//remove all buffs
		for ( int i = buffs.Count - 1; i >= 0; i-- )
		{
			Buff tempBuff = (Buff)buffs[i];
			tempBuff.End();
			//buffs.RemoveAt( i ); //If we want some to not vanish on going down.
		}
		buffs.Clear ();
		#endregion
		//Flash red.
		this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 0.75f, 0.75f), 0.05f );
		GetComponent<VibrationManager>().ScheduleVibration ( 0.35f, 0.35f, 0.10f );
		GetComponent<Animator>().Play ( "downed_" + GetAniSuffix() );
	}

	//-------------------------------------------------------------------------------------------------------
	//State Stuff
	//-------------------------------------------------------------------------------------------------------
	void ChangeState( string newState )
	{
		//Forces the state to change next frame.
		nextState = newState;
		stateTimer = 0.0f;
	}
	
	void OnStateChange( string newState )
	{
		//handles "global" state changes: shared by all classes
		if ( newState == "item windup" )
		{
			//TODO: pull out animation
		}
		else if ( newState == "item charge" )
		{
			//freeze in a loop
			stateTimer = 0.0f;
			nextState = "item charge";
			//TODO: animation
		}
		else if ( newState == "item aim ray" )
		{
			//freeze in a loop
			stateTimer = 0.0f;
			nextState = "item aim ray";
			//TODO: animation
		}
		else if ( newState == "item aim point" )
		{
			//freeze in a loop
			stateTimer = 0.0f;
			nextState = "item aim point";
			this.gameObject.GetComponent<CustomController>().aimPoint = this.gameObject.transform.position;
			//TODO: animation
		}
	}

	public string GetAniSuffix()
	{
		//returns a string to append to a base animation anem to get the correct direction.
		CustomController controller = GetComponent<CustomController>();
		if ( controller.facing == 0 )
		{
			return "right";
		}
		else if ( controller.facing == 1 )
		{
			return "up";
		}
		else if ( controller.facing == 2 )
		{
			return "left";
		}
		else if ( controller.facing == 3 )
		{
			return "down";
		}
		//default
		return "error";
	}

	public void OnHitCallback()
	{
		//this function is called when an enemy was hit by this player's attack.
		GetComponent<CustomController>().actionHandler.OnHitCallback();
	}

	public void RemoveBuffsGivenByPlayer( int giverID )
	{
		//removes all buffs given to you by the player with ID.
		for ( int i = buffs.Count - 1; i >= 0; i-- )
		{
			Buff tempBuff = (Buff) buffs[i];
			//if it's a blacklist buff, they gave it to you, and you're not hitting yourself
			if ( tempBuff.blacklist && tempBuff.giverId == giverID && id != giverID )
			{
				//remove the buff
				( (Buff) buffs[i] ).End();
				buffs.RemoveAt ( i );
				//TODO: Play blacklist sound, do notification
				//Blacklisting causes additional anti-cooperation
				GameState.cooperationAxis = Mathf.Max ( -1.0f,  GameState.cooperationAxis - 0.025f );
			}
		}
	}
}
