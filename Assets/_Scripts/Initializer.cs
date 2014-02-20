using UnityEngine;
using System.Collections;

public class Initializer : MonoBehaviour 
{
	//this class passes objects assigned in the editor to the static game state class

	#region vars
	public GameObject[] players = new GameObject[4]; //assign in pre-game screens
	public GameObject boss;                          //assign in pre-game screens
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		//Assign the GameState's references
		for ( int i = 0; i < 4; i++ )
		{
			GameState.players[i] = players[i];
		}
		GameState.boss = boss;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
}
