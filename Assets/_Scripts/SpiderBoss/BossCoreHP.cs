using UnityEngine;
using System.Collections;

public class BossCoreHP : MonoBehaviour 
{
	#region vars
	[HideInInspector]
	public BehaviorBlackboard myBlackboard;

	public Texture2D HPBarFill;
	public Texture2D HPBarBG;

	private float alpha = 1.0f;
	private float prevAlpha = 1.0f;
	ScheduledColor currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 1.0f), 0.0f );

	private float soundDelay = 0.5f; //seconds between hurt sound being played? TODO: better solution.
	private float soundTimer = 0.0f;
	#endregion

	// Use this for initialization
	void Awake () 
	{
		myBlackboard = this.gameObject.GetComponent<BehaviorBlackboard>();
		//TODO: get player count.
	}

	void Start()
	{
		//TODO: scale max HP with number of players
		myBlackboard.maxHP = 8500.0f;
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
		float HPBarWidth = 900.0f;
		GUI.DrawTexture ( new Rect( (Screen.width - HPBarWidth) / 2.0f, Screen.height - 32.0f, HPBarWidth, 32.0f ), HPBarBG );
		GUI.DrawTexture ( new Rect( (Screen.width - HPBarWidth) / 2.0f, Screen.height - 32.0f, HPBarWidth * percentHP, 32.0f ), HPBarFill );
	}

	public void Hurt( float damage, int id )
	{
		//Handles the main body taking damage.
		if ( id == -1 ) { return; } //Boss can't hurt itself
		if ( myBlackboard._invincible ) { return; } //Boss can't be hurt if invincible.
		float prevHP = myBlackboard.HP;
		myBlackboard.HP -= damage;
		ScoreManager.DealtDamage( id, damage );
		currentColor = new ScheduledColor( new Color( 1.0f, 0.5f, 0.5f), 0.25f );

		if ( prevHP > 0.0f && myBlackboard.HP <= 0.0f )
		{
			ScoreManager.LastHit ( id );
		}

		//Play hurt sound?
		if ( soundTimer <= 0.0f )
		{
			GetComponent<AudioSource>().PlayOneShot ( GetComponent<SoundStorage>().KnightSlice, 0.35f );
			soundTimer = soundDelay;
		}

		GameState.angerAxis += Mathf.Min(0.0075f , 1.0f - GameState.angerAxis);

		//increase player threat for dealing damage
		GameState.playerThreats[id] += 1.5f;

		//callback player
		GameState.playerStates[id].OnHitCallback();
	}
}
