using UnityEngine;
using System.Collections;

public class ScheduledColor
{
	//Class for holding color flashes for a time!
	public Color color;    //the color to apply
	public float duration; //duration of the color flash, in seconds.
	public float timer;    //tracked vs. duration

	public ScheduledColor( Color myColor, float myDuration )
	{
		//constructor
		color = myColor;
		duration = myDuration;
		timer = 0.0f;
	}
}

public class PlayerColor : MonoBehaviour 
{
	#region vars
	private Player player;
	public ScheduledColor currentColor = new ScheduledColor( new Color(1.0f, 1.0f, 1.0f), 0.0f );
	private ArrayList scheduledColors = new ArrayList(); //ordered list of colors. Executed in-order.
	#endregion

	// Use this for initialization
	void Awake () 
	{
		player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log ( this.gameObject.GetComponent<SpriteRenderer>().sprite.name );
		this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		//Check if player is in bullet time.
		if ( player.isInBulletTime )
		{
			this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 0.5f, 0.5f, 1.0f, 1.0f );
			this.gameObject.GetComponent<Animator>().speed = 1.0f;
		}
		else
		{
			//Set their speed to whatever time scaling should make it. (note: 1.0 t scale - normal)
			this.gameObject.GetComponent<Animator>().speed = StaticData.t_scale;
		}

		//Check if player is poisoned.
		for ( int i = 0; i < player.buffs.Count; i++)
		{
			if ( ((Buff)player.buffs[i]).regen < 0.0f )
			{
				this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 0.5f, 1.0f, 0.5f, 1.0f );
			}
		}

		//Check for any color overlays we should be displaying.
		if ( currentColor.timer >= currentColor.duration && scheduledColors.Count > 0 )
		{
			currentColor.timer = 0.0f;
			currentColor = (ScheduledColor) scheduledColors[0];
			scheduledColors.RemoveAt ( 0 );
		}

		if ( currentColor.timer < currentColor.duration ) //hasn't expired
		{
			this.gameObject.GetComponent<SpriteRenderer>().color = currentColor.color;
			float dt = Time.deltaTime;
			if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; } //handle time dilation
			currentColor.timer += dt;
			if ( currentColor.timer >= currentColor.duration )
			{
				//We would do cleanup here if it was a list.
				if ( scheduledColors.Count > 0 )
				{
					currentColor.timer = 0.0f;
					currentColor = (ScheduledColor) scheduledColors[0];
					scheduledColors.RemoveAt ( 0 );
				}
			}
		}
	}

	public void Blink()
	{
		scheduledColors.Clear ();
		const float r = 1.0f;
		const float g = 0.5f;
		const float b = 0.5f;
		const float blinkAlpha = 0.35f;
		for ( int blinks = 0; blinks < 8; blinks ++ )
		{
			scheduledColors.Add ( new ScheduledColor( new Color( r, g, b, 1.0f), 1.0f / 30.0f ) );
			scheduledColors.Add ( new ScheduledColor( new Color( r, g, b, blinkAlpha), 1.0f / 30.0f ) );
		}
	}
}
