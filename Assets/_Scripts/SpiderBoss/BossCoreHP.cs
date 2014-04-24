using UnityEngine;
using System.Collections;

public class BossCoreHP : MonoBehaviour 
{
	#region vars
	[HideInInspector]
	public BehaviorBlackboard myBlackboard;

	private const float baseFourPlayerHP = 12000.0f; //hp with 4 players.

	public Texture2D HPBarFill;
	public Texture2D HPBarBG;
	public Texture2D HPBarHolder;

	private float alpha = 1.0f;
	private float prevAlpha = 1.0f;
	ScheduledColor currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 1.0f), 0.0f );

	private float soundDelay = 0.2f; //seconds between hurt sound being played? TODO: better solution.
	private float soundTimer = 0.0f;
	#endregion

	// Use this for initialization
	void Awake () 
	{
		myBlackboard = this.gameObject.GetComponent<BehaviorBlackboard>();
	}

	void Start()
	{
		//Scale HP with player count
		myBlackboard.maxHP = baseFourPlayerHP * GameState.playerCount / 4.0f;
		//heal to full.
		myBlackboard.HP = myBlackboard.maxHP;
	}
	
	// Update is called once per frame
	void Update () 
	{
		soundTimer = Mathf.Max ( 0.0f, soundTimer - Time.deltaTime * StaticData.t_scale );
		HandleColoration();
	}

	void HandleColoration()
	{
		//function to handle color flash scheduling for the boss main body.
		//NOTE: also needs to change children's sprite renderer's colors due to composite makeup of boss.
		//NOTE: detects if a player is under it, and alpha blends if they are.
		bool decrementAlpha = false;

		CircleCollider2D[] circles = GetComponentsInChildren<CircleCollider2D>();
		foreach ( CircleCollider2D circle in circles )
		{
			foreach ( Collider2D collider in Physics2D.OverlapCircleAll( 
				new Vector2(circle.transform.position.x, circle.transform.position.y ) + circle.center, circle.radius ) )
			{
				if ( collider.gameObject.GetComponent<Player>() != null )
				{
					decrementAlpha = true;
				}
			}
		}
		if ( decrementAlpha )
		{
			alpha = Mathf.Max ( 0.35f, alpha - ( 0.25f * Time.deltaTime * StaticData.t_scale ) );
		}
		else
		{
			alpha = Mathf.Min ( 1.0f, alpha + ( 0.25f * Time.deltaTime * StaticData.t_scale ) );
		}


		if ( currentColor.duration > 0.0f )
		{
			if ( currentColor.timer >= currentColor.duration )
			{
				currentColor.duration = 0.0f;
				transform.GetComponent<SpriteRenderer>().color = getResetColor ();
				SpriteRenderer[] temp = GetComponentsInChildren<SpriteRenderer>();
				foreach ( SpriteRenderer s in temp )
				{
					s.color = getResetColor ();
				}
			}
			else
			{
				currentColor.color.a = alpha;
				transform.GetComponent<SpriteRenderer>().color = currentColor.color;
				SpriteRenderer[] temp = GetComponentsInChildren<SpriteRenderer>();
				foreach ( SpriteRenderer s in temp )
				{
					s.color = currentColor.color;
				}
			}
			currentColor.timer += Time.deltaTime * StaticData.t_scale;
		}
		else if ( alpha != prevAlpha )
		{
			transform.GetComponent<SpriteRenderer>().color = getResetColor ();
			SpriteRenderer[] temp = GetComponentsInChildren<SpriteRenderer>();
			foreach ( SpriteRenderer s in temp )
			{
				s.color = getResetColor ();
			}
		}

		prevAlpha = alpha;
	}

	Color getResetColor()
	{
		return new Color( 1.0f, 1.0f, 1.0f, alpha );
	}

	void OnGUI()
	{
		//Handles the drawing of the boss HP bar, and move names
		//TODO:
		float percentHP = Mathf.Max( myBlackboard.HP / myBlackboard.maxHP, 0.0f );
		float HPBarWidth = 800.0f;
		GUI.DrawTexture ( new Rect( (Screen.width - HPBarWidth) / 2.0f, Screen.height - 48.0f, HPBarWidth, 32.0f ), HPBarBG );
		GUI.DrawTexture ( new Rect( (Screen.width - HPBarWidth) / 2.0f, Screen.height - 48.0f, HPBarWidth * percentHP, 32.0f ), HPBarFill );
		GUI.DrawTexture ( new Rect( (Screen.width - HPBarWidth - 32.0f) / 2.0f, Screen.height - 64.0f, HPBarWidth + 32.0f, 64.0f ), HPBarHolder );
	}

	public void Hurt( float damage, int id )
	{
		//Handles the main body taking damage.
		if ( id == -1 ) { return; } //Boss can't hurt itself.
		if ( myBlackboard._invincible ) { return; } //Boss can't be hurt if invincible.
		if ( myBlackboard.HP <= 0.0f ) { return; } //Boss is dead.
		float prevHP = myBlackboard.HP;
		myBlackboard.HP -= damage;
		ScoreManager.DealtDamage( id, damage );
		currentColor = new ScheduledColor( new Color( 1.0f, 0.5f, 0.5f), 0.25f );

		if ( prevHP > 0.0f && myBlackboard.HP <= 0.0f )
		{
			ScoreManager.LastHit ( id );
			//play dead sound
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossDeathScream, 1.0f );
		}

		//Play hurt sound?
		if ( soundTimer <= 0.0f )
		{
			//play random hurt sound
			GetComponent<AudioSource>().PlayOneShot ( SoundStorage.KnightSlice, 1.0f );
			/*
			float rng = Random.Range ( 0.0f, 100.0f );
			float possibilities = 2.0f;
			if ( rng <= 1.0f * 100.0f / possibilities )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossHit1, 1.0f );
			}
			else //if ( rng <= 2.0f * 100.0f / possibilities )
			{
				GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossHit2, 1.0f );
			}
			*/
			soundTimer = soundDelay;
		}

		GameState.angerAxis += Mathf.Min(0.0075f , 1.0f - GameState.angerAxis);

		//increase player threat for dealing damage
		GameState.playerThreats[id] += 0.3f;

		//callback player
		GameState.playerStates[id].OnHitCallback();
	}
}
