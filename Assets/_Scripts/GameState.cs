using UnityEngine;
using System.Collections;

public static class GameState //: MonoBehaviour 
{
	//utility class that stores references to the game objects
	//these are set once on initialization (used as global constant references)
	#region vars
	public static GameObject[] players = new GameObject[4];  //assign in pre-game screens
	public static Player[] playerStates = new Player[4];     //assign in pre-game screens
	public static GameObject boss;                           //assign in pre-game screens
	public static GameObject[] bossLegs = new GameObject[8]; //assign in pre-game screens
	public static CameraController cameraController;         //set in inspector.
	public static bool isTutorial;                           //assign in pre-game screens
	public static int playerCount;                           //assign in pre-game screens
	#endregion

	#region BossVars
	//Character's threat levels
	//set this way because I don't want to mess with the players list as it's already being used in plenty of places.
	public static float[] playerThreats = new float[4]; //ASSUME: correspondence: index here = players[] index.
	
	//This is the axis that the boss 
	public static float cooperationAxis; //affects the usage of Dissention/Cooperation attacks
	public static float angerAxis; //affects more powerful moves to use/aggression
	#endregion
}
