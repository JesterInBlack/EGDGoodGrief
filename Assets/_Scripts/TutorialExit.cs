using UnityEngine;
using System.Collections;

public class TutorialExit : MonoBehaviour 
{

	#region vars
	private bool someoneFinishedTutorial = false;
	#endregion

	// Use this for pre-initialization
	void Awake () 
	{
		if ( ! GameState.isTutorial )
		{
			Debug.Log ( "The tutorial flag was not set. This was caught and set. (Did you skip the menu?)" );
			GameState.isTutorial = true; //In case someone starts this scene without going through the menu.
		}
	}

	// Use this for initialization
	void Start ()
	{
		someoneFinishedTutorial = false;
		GetComponent<SpriteRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//if tutorial completed & all 4 players are in this box, load the next level.
		if ( someoneFinishedTutorial )
		{
			int playersInsideMe = 0;
			BoxCollider2D box = GetComponent<BoxCollider2D>();
			float x = transform.position.x;
			float y = transform.position.y;
			Vector2 a = new Vector2( x - box.size.x / 2.0f, y - box.size.y / 2.0f );
			Vector2 b = new Vector2( x + box.size.x / 2.0f, y + box.size.y / 2.0f );
			foreach ( Collider2D c in Physics2D.OverlapAreaAll( a, b ) )
			{
				if ( c.gameObject.GetComponent<Player>() != null )
				{
					playersInsideMe++;
				}
			}

			if ( playersInsideMe == 4 ) //TODO: replace w/ # of players.
			{
				GameState.isTutorial = false;
				Application.LoadLevel ( "Master" );
			}
		}
		else
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( GameState.players[i].GetComponent<Tutorial>().completed )
				{
					someoneFinishedTutorial = true;
					GetComponent<SpriteRenderer>().enabled = true;
				}
			}
		}
	}
}
