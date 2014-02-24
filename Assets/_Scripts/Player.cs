using UnityEngine;
using System.Collections;

public enum CharacterClasses { KNIGHT, ARCHER, NINJA, DEFENDER };

public class Player : MonoBehaviour 
{

	#region vars
	public float HP;
	public float maxHP;
	public float baseMaxHP = 100.0f;

	int score = 0;

	float defense = 1.0f; //defensive power: (2x = 1/2 damage). Base: 1 = 1x damage.
	float offense = 1.0f; //offensive power: (2x = 2x  damage). Base: 1 = 1  damage.

	CharacterClasses characterclass; //enum

	//color / player id?

	//state data?
	//idle, walk, <custom via plugin>
	public string state = "idle";     //current state
	public float stateTimer = 0.0f;   //state timer
	public string nextState = "idle"; //next state: on state timer reaching 0, transition occurs.

	public bool canMove = true;           //disable moving while attacking?
	public float speedMultiplier = 1.0f;  //able to move, but at a slower pace?
	//bool overrideMoveAni;

	public bool isDodging = false;   //currently dodging?
	public float dodgeTimer = 0.0f;  //time left in dodge animation?

	public bool isParrying = false;  //currently parrying?
	public float parryTimer = 0.0f;  //time left in parry animation?

	//Downed / Carrying State Stuff
	public bool isDowned = false;    //if you're downed
	public bool isCarrier = false;   //if you're carrying another player.
	public bool isCarried = false;   //if you're being carried by another player.
	public GameObject Carried;       //ref to the player you're carrying.
	public GameObject Carrier;       //ref to the player carrying you.
	public Vector2 carryVec;         //unit vector representing your carry direction.

	//interruption
	public float interruptHP = 0.0f;   //"interrupt hp": if this reaches 0, you get interrupted. Set by moves.
	private float interruptDR = 0.0f;  //interrupt diminishing returns factor. 
	                                   //  (makes you harder to interrupt the more you get interrupted)

	//Buff / Debuff data?
	public bool isInBulletTime = false;     //Immune to the stop watch slow?
	float bulletTimeDuration = 0.0f;        //How long (real time) until time control immunity expires.

	bool isPoisoned = false;
	float poisonDPS = 1.0f;
	float poisonDuration = 0.0f;

	//Speed buff
	//Regen buff
	//Defense buff
	//Attack buff

	//items x 3
	//item cooldowns?
	public Item[] items = new Item[3];
	int itemIndex = 0;
	const int ITEM_SLOT_COUNT = 3;

	//unique mechanic data
	public float resource = 0.0f;        //chain / focus / style / sediment. Ranges from 0 to 1
	public float resourceGraceT = 0.0f; //at 0, resource begins degeneration
	private Vector3 prevPos;             //previous position, for detecting movement. (focus degen)
	#endregion

	// Use this for initialization
	void Start () 
	{
		//initialize stats

		maxHP = baseMaxHP;
		HP = maxHP;

		offense = 1.0f;
		defense = 1.0f;

		//Placeholder code: replace with setter from pre-game screens.
		characterclass = CharacterClasses.KNIGHT;
		for ( int i = 0; i < 3; i++ )
		{
			items[i] = new Item();
		}
		//END placeholder
	}
	
	// Update is called once per frame
	void Update () 
	{
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
			items[i].Update();
		}
		#endregion

		UpdateResource ( t );

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
		itemIndex = Mod( itemIndex + increment, ITEM_SLOT_COUNT );
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
		if ( isParrying ) { return; }

		//deal damage
		float finalDamage = damage / defense;
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
			//deduct points
			score -= 100;
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

	public void Interrupt( float damage )
	{
		//Attempt to interrupt the current move.
		//(Friendly player attacks you)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//TODO: diminishing returns, thresholds for moves, move interrupt power scaling
		//TODO: on successful interrupt, set canMove to true.
		interruptHP -= damage;
		interruptDR++;

		if ( interruptHP <= 0.0f )
		{
			canMove = true;
			//TODO: reset state stuff
			//TODO: set ani to idle
			//TODO: play interrupt sound
		}
	}

	public void KnockBack( float magnitude, Vector2 pos )
	{
		//Knock the player back? (based on where the attack come from)

		if ( isDowned ) { return; }
		if ( isParrying ) { return; }

		//TODO: move knockback power scaling
		//TODO: effects that minimize / reduce knockback. (blocking, parrying, dodging?)
		//TODO: interrupt stuff on knockback?
	}

	//-------------------------------------------------------------------------------------------------------
	//ITEMS?
	//-------------------------------------------------------------------------------------------------------
	public void UseItem()
	{
		if ( items[ itemIndex ].CoolDownTimer <= 0.0f )
		{
			//TODO: check cooldowns, set cooldowns on use.
			StopWatch();
			items[ itemIndex ].CoolDownTimer = items[ itemIndex ].CoolDownDelay;
		}
		else
		{
			//Still on cooldown.
		}
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

	#region player resources
	private void UpdateResource( float dt )
	{
		if ( characterclass == CharacterClasses.KNIGHT )
		{
			UpdateChain ( dt );
		}
		else if ( characterclass == CharacterClasses.ARCHER )
		{
			UpdateFocus ( dt );
		}
		else if ( characterclass == CharacterClasses.NINJA )
		{
			UpdateStyle ( dt );
		}
		else if ( characterclass == CharacterClasses.DEFENDER )
		{
			UpdateAccumulation ( dt );
		}
	}

	private void UpdateChain( float dt )
	{
		if ( state == "idle" || state == "walk" )
		{
			//degen grace timer
			resourceGraceT -= dt;
		}

		if ( resourceGraceT <= 0.0f )
		{
			resource = Mathf.Max ( resource - 1.0f * dt, 0.0f );
		}
	}

	private void UpdateFocus( float dt )
	{
		//Gain focus while standing still.
		//TODO: EXCEPTION FOR KNOCKBACK?
		if ( prevPos == this.gameObject.transform.position )
		{
			resource = Mathf.Min( resource + 0.5f * dt, 1.0f );
		}
		else
		{
			resource = Mathf.Max ( resource - 1.0f * dt, 0.0f );
		}
	}

	private void UpdateStyle( float dt )
	{
		//Gain style by doing stuff.
		//Lose style for not doing anything for a while.
		if ( state == "idle" || state == "walk" )
		{
			//degen grace timer
			resourceGraceT -= dt;
		}

		if ( resourceGraceT <= 0.0f )
		{
			resource = Mathf.Max ( resource - 1.0f * dt, 0.0f );
		}
	}

	private void UpdateAccumulation( float dt )
	{
		//Constantly gain sediment accumulation.
		//Some moves degenerate it.
		resource = Mathf.Min ( resource + 0.25f * dt, 1.0f );
	}

	#endregion
}
