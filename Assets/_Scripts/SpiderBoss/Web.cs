using UnityEngine;
using System.Collections;

public class Web : MonoBehaviour 
{
	#region vars
	private float debuffRate = 0.25f; //seconds between applications of the slow debuff
	private float lifespan = 10.0f;
	private float maxSize = 4.25f;

	private float lifeTimer = 0.0f;
	private float timer = 0.0f;
	
	private float growInTime = 0.25f;
	private float fadeOutTime = 0.75f;
	#endregion

	// Use this for initialization
	void Start () 
	{
		transform.localScale = new Vector3(0.0f, 0.0f, 0.0f); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		//grow in.
		float temp = Mathf.Lerp ( 0.0f, maxSize, Mathf.Min( 1.0f, lifeTimer / growInTime) );
		transform.localScale = new Vector3( temp, temp, temp );

		//fade out
		temp = Mathf.Lerp( 1.0f, 0.0f, Mathf.Min ( 1.0f, (lifeTimer - (lifespan - fadeOutTime)) / fadeOutTime ) );
		this.gameObject.GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, temp );

		#region timer
		bool applySlow = false;
		timer += Time.deltaTime * StaticData.t_scale;
		lifeTimer += Time.deltaTime * StaticData.t_scale;
		if ( lifeTimer <= lifespan - fadeOutTime )
		{
			if ( timer > debuffRate )
			{
				timer -= debuffRate;
				applySlow = true;
			}
		}

		if ( lifeTimer >= lifespan )
		{
			Destroy( this.gameObject );
		}
		#endregion

		if ( ! applySlow ) { return; } //slight optimization

		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				Player player = GameState.playerStates[i];
				//get distance from player's center to the hit circle's center
				BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
				Vector3 theirPos = player.gameObject.transform.position;
				CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
				Vector3 myPos = this.gameObject.transform.position;
				float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
				float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
				float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );
				
				if ( dist <= circle.radius * (transform.localScale.x * 0.9f)) //if player is in the region (collider center point in radius)
				{
					if ( ! player.isCarried ) //if the player is not being carried
					{
						//Slow them!
						player.Slow ( 1.0f, 0.65f );
					}
				}
			}
		}
	}
}
