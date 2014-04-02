using UnityEngine;
using System.Collections;

public enum CharacterClasses { KNIGHT, ARCHER, NINJA, DEFENDER };

public class Player : MonoBehaviour 
{
	//TODO: fix throw ani for item use.

	#region vars
	[HideInInspector]
	public int id; //used for identity checks.

	public float HP;
	[HideInInspector]
	public float maxHP;
	[HideInInspector]
	public float baseMaxHP = 100.0f;

	public float score = 0;

	public float defense = 1.0f;  //defensive power: (2x = 1/2 damage). Base: 1 = 1x damage.
	public float offense = 1.0f;  //offensive power: (2x = 2x  damage). Base: 1 = 1  damage.

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
	public float speedMultiplier = 1.0f;  //able to move, but at a slower pace?
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

	//interruption
	public float interruptHP = 0.0f;   //"interrupt hp": if this reaches 0, you get interrupted. Set by moves.
	private float interruptDR = 0.0f;  //interrupt diminishing returns factor. 
	                                   //  (makes you harder to interrupt the more you get interrupted)

	//Buff / Debuff data?
	public bool isInBulletTime = false;     //Immune to the stop watch slow?
	float bulletTimeDuration = 0.0f;        //How long (real time) until time control immunity expires.

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
	private Vector3 prevPos;             //previous position, for detecting movement. (focus degen)
	#endregion

	// Use this for initialization
	void Start () 
	{
		//initialize stats
		//GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f ); //Change this to alter colors per player

		maxHP = baseMaxHP;
		HP = maxHP;

		offense = 1.0f;
		defense = 1.0f;

		//Placeholder code: replace with setter from pre-game screens.
		//characterclass = CharacterClasses.KNIGHT;
		for ( int i = 0; i < 3; i++ )
		{
			items[i] = new Item();
		}
		items[0].Construct ( ItemName.STOPWATCH );
		items[1].Construct ( ItemName.PHEROMONE_JAR );
		items[2].Construct ( ItemName.VAMPIRE_FANG );
		//END placeholder

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
		#endregion

		//trap state changes
		if ( prevState != state )
		{
			OnStateChange( state );
		}
		prevState = state;

		prevPos = this.gameObject.transform.position;
	}

	void Respawn()
	{
		//state reset?
		maxHP = baseMaxHP;
		HP = maxHP;
	}

	public void ChangeItemIndex( int increment )
	{
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
			return; 
		}
		if ( isStoneSkin ) 
		{ 
			ScoreManager.AvoidedDamage ( id, damage );
			isStoneSkin = false; 
			return; 
		}
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			ScoreManager.AvoidedDamage ( id, damage );
			this.gameObject.GetComponent<StoneFist>().OnHitCallback( -1, damage );
			return; 
		}
		//TODO: add stone skin knockback handling on hit logic (somewhere?)

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
			//deadz.
			//Go into downed state.
			isDowned = true;
			//interrupt any attacks or actions. Force state change.
			state = "idle";
			stateTimer = 0.0f;
			nextState = "idle";
			//deduct points
			score -= score * 0.25f; //25%
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
			//TODO: animation
		}

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
			if ( attackerId != -1 )
			{
				//TODO: knock them back! alot!
			}
			isStoneSkin = false;
			return; 
		}
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			this.gameObject.GetComponent<StoneFist>().OnHitCallback( attackerId, damage );
			return; 
		}

		//HP -= damage; //TODO: remove
		//Flash red.
		this.gameObject.GetComponent<PlayerColor>().currentColor = new ScheduledColor( new Color(1.0f, 0.75f, 0.75f), 0.05f );

		//TODO: diminishing returns, thresholds for moves, move interrupt power scaling
		//TODO: on successful interrupt, set canMove to true.
		interruptHP -= damage;
		interruptDR++;

		if ( interruptHP <= 0.0f )
		{
			canMove = true;
			//TODO: set ani to idle
			//TODO: play interrupt sound
			state = "idle";
			stateTimer = 0.0f;
			nextState = "idle";
		}

		//TODO: Add a threshold before auras blacklist you
		//Go through the player's buffs, and remove any you have given them
		Player aggressor = GameState.players[attackerId].GetComponent<Player>();
		for ( int i = aggressor.buffs.Count - 1; i >= 0; i-- )
		{
			Buff tempBuff = (Buff) aggressor.buffs[i];
			//if it's a blacklist buff, you gave it to them, and you're not hitting yourself
			if ( tempBuff.blacklist && tempBuff.giverId == id && id != attackerId)
			{
				//remove the buff
				( (Buff) aggressor.buffs[i] ).End();
				aggressor.buffs.RemoveAt ( i );
				//TODO: Play blacklist sound, do notification
				Debug.Log ( "Player " + attackerId + " was blacklisted by Player " + id );
			}
		}
	}

	public void KnockBack( float magnitude, Vector2 pos )
	{
		//Knock the player back? (based on where the attack come from)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }
		if ( false ) { return; } //TODO: stone skin
		if ( state == "ycharge" && characterclass == CharacterClasses.DEFENDER ) 
		{ 
			//this.gameObject.GetComponent<StoneFist>().OnHitCallback( damage );
			return; 
		}

		//TODO: move knockback power scaling
		//TODO: effects that minimize / reduce knockback. (blocking, parrying, dodging?)
		//TODO: interrupt stuff on knockback?
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

	//-------------------------------------------------------------------------------------------------------
	//ITEMS?
	//-------------------------------------------------------------------------------------------------------
	public void BeginUseItem()
	{
		if ( items[ itemIndex ].coolDownTimer <= 0.0f )
		{
			if ( items[ itemIndex ].itemType == ItemType.ITEM_FAST )
			{
				//Do the effect. NOW!
				//TODO: freeze item scrolling / use during item use animation / charging
				state = "item windup";
				stateTimer = 0.05f * 12.0f; //frames
				nextState = "idle";
				UseItem2 ( itemIndex );
			}
			else if ( items[ itemIndex ].itemType == ItemType.ITEM_CHARGE_AND_RELEASE )
			{
				//Set up holding state, do the effect on release.
				state = "item windup";
				stateTimer = 0.05f * 2.0f; //frames
				nextState = "item charge";
			}
			else if ( items[ itemIndex ].itemType == ItemType.ITEM_AIM_RAY)
			{
				//Set up aiming state, do the effect on release.
				state = "item windup";
				stateTimer = 0.05f * 2.0f; //frames
				nextState = "item aim ray";
			}
			else if ( items[ itemIndex ].itemType == ItemType.ITEM_AIM_POINT )
			{
				//Set up aiming state, do the effect on release.
				state = "item windup";
				stateTimer = 0.05f * 2.0f; //frames
				nextState = "item aim point";
			}
		}
		else
		{
			//Still on cooldown.
		}
	}

	public void EndUseItem()
	{
		if ( items[ itemIndex ].coolDownTimer <= 0.0f )
		{
			UseItem2 ( itemIndex );
		}
	}

	public void UseItem2( int index )
	{
		//This function is called when an item's effect is to take place.
		//IE: this comes after the charging / aiming junk
		//it does the effect + sets the cooldown

		items[ index ].coolDownTimer = items[ index ].coolDownDelay; //set cooldown
		#region effects
		//DO EFFECT:
		if ( items[index].name == ItemName.STOPWATCH )
		{
			StopWatch ();
		}
		else if ( items[index].name == ItemName.AURA_DEFENSE )
		{
			const float duration = 60.0f;
			Aura ( id, duration, 0.0f, 1.0f, 0.0f );
		}
		else if ( items[index].name == ItemName.AURA_OFFENSE )
		{
			const float duration = 60.0f;
			Aura ( id, duration, 1.0f, 0.0f, 0.0f );
		}
		else if ( items[index].name == ItemName.AURA_REGEN )
		{
			const float duration = 60.0f;
			Aura ( id, duration, 0.0f, 0.0f, 1.0f );
		}
		else if ( items[index].name == ItemName.VAMPIRE_FANG )
		{
			//TODO: charge
			//TODO: hit detection box at position after animating:
			//on success: trigger suck life
			float angle = GetComponent<CustomController>().facing * Mathf.PI / 2.0f;
			float x = transform.position.x;
			float y = transform.position.y;
			float min = 0.5f; //minimum or base width & height of the hitbox
			float r = 1.0f;   //factor applied to cos / sin to extend hitbox
			//
			float xmin = Mathf.Min ( x - min, x - min + r * Mathf.Cos ( angle ) );
			float ymin = Mathf.Min ( y - min, y - min + r * Mathf.Sin ( angle ) );
			float xmax = Mathf.Max ( x + min, x + min + r * Mathf.Cos ( angle ) );
			float ymax = Mathf.Max ( y + min, y + min + r * Mathf.Sin ( angle ) );
			if ( AttackSystem.getHitsInBox( new Rect( xmin, ymin, (xmax - xmin), (ymax - ymin) ), id ).Length > 0 )
			{
				state = "vampire";
				stateTimer = 1.5f;
				nextState = "idle";
				canMove = false;
			}
		}
		else if ( items[index].name == ItemName.PHEROMONE_JAR )
		{
			//TODO: hit detection circle at point after animating
			//TODO: animate
			//TODO: put gas + glass prefab
			foreach (Collider2D hit in AttackSystem.getHitsInCircle( GetComponent<CustomController>().aimPoint, 1.0f, id ) )
			{
				Player tempPlayer = hit.gameObject.GetComponent<Player>();
				if ( tempPlayer != null )
				{
					//Max out player threat!
					GameState.playerThreats[ tempPlayer.id ] = 1.0f;
				}
			}
			//State stuff
			state = "item windown"; //item wind down?
			stateTimer = 0.05f * 12.0f; //frames for animation.
			nextState = "idle";
		}
		#endregion
		#region animation
		gameObject.GetComponent<Animator>().Play( "throw_" +  GetAniSuffix() );
		#endregion
	}

	private void StopWatch()
	{
		isInBulletTime = true;
		bulletTimeDuration = 30.0f;
		StaticData.t_scale = 0.5f;
		StaticData.bulletTimeDuration = bulletTimeDuration;
		//TODO: lerp in, add visual effect, play sound
		//TODO: duration on t scale, lerp back in, remove visual effect, play sound
	}

	private void Aura( int id, float duration, float offense, float defense, float regen )
	{
		//Give all players the blacklist buff.
		//TODO: sound?
		//TODO: don't allow players to stack the same buff from the same source multiple times on themselves.
		//      instead, refresh it.
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			Player targetPlayer = GameState.players[i].GetComponent<Player>();
			Buff myBuff = new Buff();
			myBuff.player = targetPlayer;
			myBuff.giverId = id;
			myBuff.blacklist = true;
			myBuff.offense = offense;
			myBuff.defense = defense;
			myBuff.regen = regen;
			myBuff.duration = duration;
			//add to player and apply effect
			//FIRST: change stacking from the same source -> refresh 
			//remove any old buffs that match it with the same source
			//(so you can't multi-stack the same aura on yourself from yourself 3 times,
			// 12x of the same aura buffs is a scaling nightmare, and would require nerfing auras to obsolescence)
			//Also, this lets us make the CD shorter than the duration
			for ( int j = targetPlayer.buffs.Count - 1; j >= 0; j-- )
			{
				if ( ( (Buff) targetPlayer.buffs[j] ).isTheSameAs( myBuff ) )
				{
					( (Buff) targetPlayer.buffs[j] ).End();
					//Debug.Log ( "Buff refreshed on player " + (i + 1) );
					targetPlayer.buffs.RemoveAt ( j );
				}
			}
			GameState.players[i].GetComponent<Player>().buffs.Add ( myBuff );
			myBuff.Start ();
			//Debug.Log ( "Buff started on player " + (i + 1) );
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
}
