using UnityEngine;
using System.Collections;

public class MenuDataSaver : MonoBehaviour 
{
	#region vars
	public bool[] playersConnected = new bool[4];
	public CharacterClasses[] playerClasses = new CharacterClasses[4];
	public ItemName[,] playerItems = new ItemName[4, 3];
	#endregion

	//Use this for pre-initialization
	void Awake ()
	{
		DontDestroyOnLoad ( this.gameObject );
		DontDestroyOnLoad ( this );
	}
}
