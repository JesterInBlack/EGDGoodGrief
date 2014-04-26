using UnityEngine;
using System.Collections;

public class GameStateWatcher : MonoBehaviour 
{
	//Game State Watcher: polls game state, checks if the game has been won / lost
	//(ie. all players are dead / boss is dead.)

	#region vars
	public GameObject blackout;

	private float lostT = 0.0f;
	#endregion
	
	// Update is called once per frame
	void Update () 
	{
		ScoreManager.UpdateScores();
		if ( GameState.boss == null || GameState.isTutorial ) { return; } //tutorial

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
		//if ( GameState.boss.GetComponent<BossCoreHP>().myBlackboard.HP <= 0 )
		if ( GameState.boss.GetComponent<BossCoreHP>().myBlackboard._moveToEndScreen )
		{
			won = true;
		}
		#endregion

		if ( lost )
		{
			//Exit to loss screen
			//Black out first.
			if ( lostT == 0.0f )
			{
				Instantiate ( blackout, this.gameObject.transform.position, Quaternion.identity );
			}
			else if ( lostT >= 1.5f )
			{
				Application.LoadLevel ( "Loss" );
			}
			//Debug.Log ( "Total Party Kill" );
			lostT += Time.deltaTime * StaticData.t_scale;
		}
		if ( won )
		{
			//Exit to victory screen
			//TODO: show won / lost GUI first.
			//Debug.Log( "Victory" );
			Application.LoadLevel( "Score" );
		}
	}
}
