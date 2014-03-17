using UnityEngine;
using System.Collections;

public class GameStateWatcher : MonoBehaviour 
{
	//Game State Watcher: polls game state, checks if the game has been won / lost
	//(ie. all players are dead / boss is dead.)

	#region vars
	#endregion

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region Lose Check
		bool lost = true;
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				if ( ! GameState.players[i].GetComponent<Player>().isDowned )
				{
					lost = false;
				}
			}
		}
		#endregion

		#region Win Check
		bool won = false;
		if ( GameState.boss.GetComponent<Boss>().hp <= 0 )
		{
			//TODO: restructure
			won = true;
		}
		#endregion

		if ( lost )
		{
			//Exit to loss screen
			//TODO: show won / lost GUI first.
			Debug.Log ( "Total Party Kill" );
		}
		if ( won )
		{
			//Exit to victory screen
			//TODO: show won / lost GUI first.
			Debug.Log( "Victory" );
		}
	}
}
