using UnityEngine;
using System.Collections;

public class ScorePasser : MonoBehaviour 
{
	//Component to pass score data between scenes

	#region vars
	[HideInInspector]
	public ScoreDetails scoreDetails = new ScoreDetails(); //written to by scoremanager/player/gamestatewatcher.
	#endregion

	// Use this for initialization
	void Awake () 
	{
		DontDestroyOnLoad ( this.gameObject );
		DontDestroyOnLoad ( this );
	}
}
