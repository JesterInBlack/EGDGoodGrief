using UnityEngine;
using System.Collections;

public static class GameState //: MonoBehaviour 
{
	//utility class that stores references to the game objects
	//these are set once on initialization (used as global constant references)
	#region vars
	public static GameObject[] players = new GameObject[4]; //assign in pre-game screens
	public static GameObject boss;                          //assign in pre-game screens
	#endregion
}
