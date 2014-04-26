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
		if(Input.GetKeyDown(KeyCode.UpArrow))
		{
			GameState.angerAxis += 0.1f;
		}
		if(Input.GetKeyDown(KeyCode.DownArrow))
		{
			GameState.angerAxis -= 0.1f;
		}
		if(Input.GetKeyDown(KeyCode.LeftArrow))
		{
			GameState.cooperationAxis -= 0.1f;
		}
		if(Input.GetKeyDown(KeyCode.RightArrow))
		{
			GameState.cooperationAxis += 0.1f;
		}

		if(Input.GetKeyDown(KeyCode.D))
		{
			if(GetComponent<GUIText>().enabled == false)
			{
				GetComponent<GUIText>().enabled = true;
			}
			else
			{
				GetComponent<GUIText>().enabled = false;
			}
		}

		string text = "Coop: " + GameState.cooperationAxis + 
			"\nAnger: " + GameState.angerAxis + 
			"\nThreat: " + 
			(int)GameState.playerThreats[0] + ", " + 
			(int)GameState.playerThreats[1] + ", " + 
			(int)GameState.playerThreats[2] + ", " + 
			(int)GameState.playerThreats[3] +
			"\nCurrentBehavior: " + GameState.boss.GetComponent<BossManager>()._currentBehavior;
		myText.text = text;
	}
}
