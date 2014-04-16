using UnityEngine;
using System.Collections;

public class BossDebugHelper : MonoBehaviour 
{
	#region vars
	GUIText myText;
	#endregion

	// Use this for pre-initialization
	void Awake ()
	{
		myText = GetComponent<GUIText>();
	}

	// Use this for initialization
	void Start () 
	{
	}
	
	// Update is called once per frame
	void Update () 
	{
		string text = "Coop: " + GameState.cooperationAxis + 
			"\nAnger: " + GameState.angerAxis + 
			"\nThreat: " + 
			(int)GameState.playerThreats[0] + ", " + 
			(int)GameState.playerThreats[1] + ", " + 
			(int)GameState.playerThreats[2] + ", " + 
			(int)GameState.playerThreats[3];
		myText.text = text;
	}
}
