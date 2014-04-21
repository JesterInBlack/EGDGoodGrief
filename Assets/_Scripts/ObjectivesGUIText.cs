using UnityEngine;
using System.Collections;

public class ObjectivesGUIText : MonoBehaviour 
{

	#region vars
	bool someoneFinishedTutorial = false;
	float timer = 0.0f;
	string text = "Objectives:\nLearn the ropes";
	string text2 = "Objectives:\nExit the Tutorial\nKill the Boss\nOutperform Your Allies";
	#endregion

	// Use this for initialization
	void Start () 
	{
		someoneFinishedTutorial = false;
		timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime;
		if ( someoneFinishedTutorial )
		{
			float lerpT = Mathf.Min( 1.0f, timer / (text2.Length * 0.05f) );
			float length = lerpT * text2.Length;
			GetComponent<GUIText>().text = text2.Substring ( 0, (int)length );
		}
		else
		{
			float lerpT = Mathf.Min ( 1.0f, timer / (text.Length * 0.05f) );
			float length = lerpT * text.Length;
			GetComponent<GUIText>().text = text.Substring ( 0, (int)length );
			for ( int i = 0; i < GameState.players.Length; i++ )
			{
				if ( GameState.players[i] != null )
				{
					if ( GameState.players[i].GetComponent<Tutorial>().completed )
					{
						timer = 0.0f; //reset
						someoneFinishedTutorial = true;
					}
				}
			}
		}
	}
}
