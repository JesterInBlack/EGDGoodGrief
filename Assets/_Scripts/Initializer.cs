using UnityEngine;
using System.Collections;

public class Initializer : MonoBehaviour 
{
	//this class passes objects assigned in the editor to the static game state class
	//changed width / height to be in tiles.

	#region vars
	public GameObject[] players = new GameObject[4];  //assign in pre-game screens
	public GameObject bossPrefab;
	//public GameObject spawnedPrefab;
	public GameObject boss;                           //assign in pre-game screens
	public GameObject[] bossLegs = new GameObject[8]; //assign in pre-game screens
	public GameObject cameraController;               //set in inspector.

	//Tiles
	public GameObject tileFolder;                     //assign in editor
	public GameObject tilePrefab;                     //assign in editor
	public Vector2 levelSize;                         //level width / height: assign in editor
	public GameObject wallPrefab;                     //assign in editor

	private const float tilesToUnits = 64.0f / 48.0f;  //conversion factor (tile coords -> unity units)
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		//spawnedPrefab = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		bossPrefab = GameObject.Find("BossTest");
		boss = bossPrefab.transform.Find("BodyParts/MainBody").gameObject;

		//Assign the GameState's references
		for ( int i = 0; i < 4; i++ )
		{
			GameState.players[i] = players[i];
			GameState.playerStates[i] = players[i].GetComponent<Player>();
			players[i].GetComponent<Player>().id = i;
		}

		if ( boss != null )
		{
			GameState.boss = boss;
			for ( int i = 0; i < 8; i++ )
			{
				GameState.bossLegs[i] = bossLegs[i];
				//TODO: set ids?
			}
		}
		GameState.cameraController = cameraController.GetComponent<CameraController>();

		//round to int.
		levelSize.x = (int) levelSize.x;
		levelSize.y = (int) levelSize.y;

		//Tiles are (64.0f / 48.0f) units big.
		for ( float x = levelSize.x / -2.0f; x <= levelSize.x / 2.0f; x ++ )
		{
			for ( float y = levelSize.y / -2.0f; y <= levelSize.y / 2.0f; y ++ )
			{
				GameObject obj = (GameObject)Instantiate ( tilePrefab,  new Vector3( x * tilesToUnits, y * tilesToUnits, 0.0f), Quaternion.identity );
				obj.transform.parent = tileFolder.transform;
			}
		}

		//Walls on the edge of the map
		for ( float x = levelSize.x / -2.0f - (1.0f); x <= levelSize.x / 2.0f + (1.0f); x ++ )
		{
			GameObject obj;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( x * tilesToUnits, (levelSize.y / -2.0f - 1.0f) * tilesToUnits, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( x * tilesToUnits, (levelSize.y /  2.0f + 1.0f) * tilesToUnits, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
		}
		for ( float y = levelSize.y / -2.0f - (1.0f); y <= levelSize.y / 2.0f + (1.0f); y ++ )
		{
			GameObject obj;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( (levelSize.x / -2.0f - 1.0f) * tilesToUnits, y * tilesToUnits, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
			obj = (GameObject)Instantiate ( wallPrefab, new Vector3( (levelSize.x /  2.0f + 1.0f) * tilesToUnits, y * tilesToUnits, 0.0f), Quaternion.identity );
			obj.transform.parent = tileFolder.transform;
		}

		//get player count.
		GameObject dataObj = GameObject.Find ( "MenuDataSaver" );
		if ( dataObj != null )
		{
			MenuDataSaver menuData = dataObj.GetComponent<MenuDataSaver>();
			int playerCount = 0;
			for ( int i = 0; i < menuData.playersConnected.Length; i++ )
			{
				if ( menuData.playersConnected[i] ) { playerCount ++; }
			}
			GameState.playerCount = playerCount;
			menuData.playerCountRead = true;
		}
		else
		{
			GameState.playerCount = 4;
		}
	}

	// Use this for initialization
	void Start () 
	{
		ItemImages.Start();
	}
}
