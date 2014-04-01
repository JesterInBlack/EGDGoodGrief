using UnityEngine;
using System.Collections;

public class Web : MonoBehaviour 
{
	#region vars
	private float debuffRate = 0.25f; //seconds between applications of the slow debuff

	private float timer = 0.0f;
	
	private float growValue;
	private float growRate;
	#endregion

	// Use this for initialization
	void Start () 
	{
		growRate = 4.0f;
		growValue = 0.0f;
		transform.localScale = new Vector3(growValue, growValue, growValue); 
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(growValue < 1.0f)
		{
			growValue += Time.deltaTime * growRate * StaticData.t_scale;
			transform.localScale = new Vector3(growValue, growValue, growValue);
		}

		#region timer
		bool applySlow = false;
		timer += Time.deltaTime * StaticData.t_scale;
		if ( timer > debuffRate )
		{
			timer -= debuffRate;
			applySlow = true;
		}
		#endregion

		if ( ! applySlow ) { return; } //slight optimization

		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			Player player = GameState.players[i].GetComponent<Player>();
			//get distance from player's center to the hit circle's center
			BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
			Vector3 theirPos = player.gameObject.transform.position;
			CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
			Vector3 myPos = this.gameObject.transform.position;
			float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
			float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
			float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );
			
			if ( dist <= circle.radius ) //if player is in the region (collider center point in radius)
			{
				if ( ! player.isCarried ) //if the player is not being carried
				{
					//Slow them!
					player.Slow ( 1.0f, 0.60f );

				}
			}
		}
	}
}
