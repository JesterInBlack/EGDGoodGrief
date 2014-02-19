using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	//this class tracks the position of the players, and "zooms" the camera in and out to keep them all onscreen.


	#region vars
	public GameObject[] players = new GameObject[4]; //list of players, for position tracking
	#endregion

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region follow
		//TODO: make it follow the avg. position of the players.
		float avg_x = 0.0f;
		float avg_y = 0.0f;
		float playercount = 0.0f;

		for ( int i = 0; i < 4; i++ )
		{
			if ( players[i] != null )
			{
				avg_x += players[i].transform.position.x;
				avg_y += players[i].transform.position.y;
				playercount ++;
			}
		}
		avg_x = avg_x / playercount;
		avg_y = avg_y / playercount;

		Camera.main.transform.position = new Vector3( avg_x, avg_y, -10.0f ); 
		#endregion

		#region zoom
		//fit to max dist in y from center, max dist in x from center. Floor on zoom in.
		float ratio = 1.67f; //factor to divide x by (aspect ratio)

		float max_dist_x = 0.0f;
		float max_dist_y = 0.0f;

		for ( int i = 0; i < 4; i++ )
		{
			if ( players[i] != null )
			{
				max_dist_x = Mathf.Max ( 
				                        Mathf.Abs( Camera.main.transform.position.x - players[i].transform.position.x ) / ratio, 
				                        max_dist_x 
				                        );
				max_dist_y = Mathf.Max ( 
				                        Mathf.Abs( Camera.main.transform.position.y - players[i].transform.position.y ), 
				                        max_dist_y 
				                        );
			}
		}
		float max_dist = Mathf.Max ( max_dist_x, max_dist_y );

		Camera.main.orthographicSize = Mathf.Max ( 1.0f, max_dist ); //catures a unit circle in the y, 1.67 in the x
		#endregion
	}
}
