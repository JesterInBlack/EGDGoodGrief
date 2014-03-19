using UnityEngine;
using System.Collections;

public class Initializer : MonoBehaviour 
{
	//this class passes objects assigned in the editor to the static game state class

	#region vars
	public GameObject[] players = new GameObject[4]; //assign in pre-game screens
	public GameObject boss;                          //assign in pre-game screens

	//Tiles
	public GameObject tileFolder;                    //assign in editor
	public GameObject tilePrefab;                    //assign in editor
	public Vector2 levelSize;                        //level width / height: assign in editor
	public GameObject wallPrefab;                    //assign in editor
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		//Assign the GameState's references
		for ( int i = 0; i < 4; i++ )
		{
			GameState.players[i] = players[i];
			players[i].GetComponent<Player>().id = i;
		}
		GameState.boss = boss;

		//Tiles are (64.0f / 48.0f) units big.
		for ( float x = levelSize.x / -2.0f; x <= levelSize.x / 2.0f; x += 64.0f / 48.0f )
		{
			for ( float y = levelSize.y / -2.0f; y <= levelSize.y / 2.0f; y += 64.0f / 48.0f )
			{
				GameObject obj = (GameObject)Instantiate ( tilePrefab,  new Vector3( x, y, 0.0f), Quaternion.identity );
				obj.transform.parent = tileFolder.transform;
			}
		}

		//Walls on the edge of the map
		for ( float x = levelSize.x / -2.0f - (64.0f / 48.0f); x <= levelSize.x / 2.0f + (64.0f / 48.0f); x += 64.0f / 48.0f )
		{
			GameObject obj = (GameObject)Instantiate ( wallPrefab, new Vector3( x, levelSize.y / -2.0f - (64.0f / 48.0f), 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( x, levelSize.y / 2.0f + (64.0f / 48.0f), 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
		}
		for ( float y = levelSize.y / -2.0f - (64.0f / 48.0f); y <= levelSize.y / 2.0f + (64.0f / 48.0f); y += 64.0f / 48.0f )
		{
			GameObject obj = (GameObject)Instantiate ( wallPrefab, new Vector3( levelSize.x / -2.0f - (64.0f / 48.0f), y, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( levelSize.x / 2.0f + (64.0f / 48.0f), y, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
		}
	}

	// Use this for initialization
	void Start () 
	{
	
	}
}
