using UnityEngine;
using System.Collections;

public class MenuDataSaver : MonoBehaviour 
{
	#region vars
	public bool tutorial = false;
	public bool[] playersConnected = {false, false, false, false};
	public CharacterClasses[] playerClasses = new CharacterClasses[4];
	public ItemName[,] playerItems = new ItemName[4, 3];

	public bool[] read = new bool[4]; //whether or not the data has been extracted.
	#endregion

	//Use this for pre-initialization
	void Awake ()
	{
		DontDestroyOnLoad ( this.gameObject );
		DontDestroyOnLoad ( this );
	}

	void Update()
	{
		//delete the object if all the data's been read
		bool deleteMe = true;
		for ( int i = 0; i < 4; i++ )
		{
			if ( ! read[i] )
			{
				deleteMe = false;
			}
		}

		if ( deleteMe )
		{
			if ( tutorial ) //don't delete in the tutorial, but prep for the actual game
			{
				tutorial = false;
				for ( int i = 0; i < 4; i++ )
				{
					read[i] = false;
				}
			}
			else //delete
			{
				GameObject.Destroy ( this.gameObject );
			}
		}
	}
}
