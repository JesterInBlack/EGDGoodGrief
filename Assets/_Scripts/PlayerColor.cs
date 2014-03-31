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
	#endregion

	// Use this for initialization
	void Awake () 
	{
		player = this.gameObject.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		//Check if player is poisoned.
		for ( int i = 0; i < player.buffs.Count; i++)
		{
			if ( ((Buff)player.buffs[i]).regen < 0.0f )
			{
				this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 0.5f, 1.0f, 0.5f, 1.0f );
			}
		}

		//Check for any color overlays we should be displaying.
		if ( currentColor.timer < currentColor.duration ) //hasn't expired
		{
			this.gameObject.GetComponent<SpriteRenderer>().color = currentColor.color;
			float dt = Time.deltaTime;
			if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; } //handle time dilation
			currentColor.timer += dt;
			if ( currentColor.timer >= currentColor.duration )
			{
				//We would do cleanup here if it was a list.
			}
		}
	}
}
