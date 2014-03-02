using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	//this class tracks the position of the players, and "zooms" the camera in and out to keep them all onscreen.


	#region vars	
	public float min_orthographic_size = 4.0f; // "maximum zoom factor"

	//"looped" arrays for smoothing using simple filtering
	private Vector3[] cameraPositions = new Vector3[10];
	private int cameraPositionOverwriteIndex = 0;
	private float[] cameraSizes = new float[5];
	private int cameraSizeOverwriteIndex = 0;
	#endregion

	// Use this for initialization
	void Start () 
	{
		//fill the camera position blending vector
		for ( int i = 0; i < cameraPositions.Length; i++ )
		{
			cameraPositions[i] = new Vector3( Camera.main.transform.position.x, Camera.main.transform.position.y, -10.0f );
		}

		//fill the camera zoom blending vector
		for ( int i = 0; i < cameraSizes.Length; i++ )
		{
			cameraSizes[i] = Camera.main.orthographicSize;
		}
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
			if ( GameState.players[i] != null )
			{
				avg_x += GameState.players[i].transform.position.x;
				avg_y += GameState.players[i].transform.position.y;
				playercount ++;
			}
		}
		avg_x = avg_x / playercount;
		avg_y = avg_y / playercount;

		cameraPositions[ cameraPositionOverwriteIndex ] = new Vector3( avg_x, avg_y, -10.0f );
		//Camera.main.transform.position = new Vector3( avg_x, avg_y, -10.0f ); 
		#endregion

		#region zoom
		//fit to max dist in y from center, max dist in x from center. Floor on zoom in.
		float ratio = 1.67f; //factor to divide x by (aspect ratio)

		float max_dist_x = 0.0f;
		float max_dist_y = 0.0f;

		for ( int i = 0; i < 4; i++ )
		{
			if ( GameState.players[i] != null )
			{
				max_dist_x = Mathf.Max ( 
				                        Mathf.Abs( Camera.main.transform.position.x - GameState.players[i].transform.position.x ) / ratio, 
				                        max_dist_x 
				                        );
				max_dist_y = Mathf.Max ( 
				                        Mathf.Abs( Camera.main.transform.position.y - GameState.players[i].transform.position.y ), 
				                        max_dist_y 
				                        );
			}
		}
		float max_dist = Mathf.Max ( max_dist_x, max_dist_y );

		cameraSizes[ cameraSizeOverwriteIndex ] = Mathf.Max ( min_orthographic_size, max_dist + 0.5f ); //captures a unit circle in the y, 1.67 in the x
		//Camera.main.orthographicSize = Mathf.Max ( min_orthographic_size, max_dist + 0.5f ); //captures a unit circle in the y, 1.67 in the x
		#endregion

		#region filter
		//averages the camera's target position over a few frames for a smoothing effect.
		avg_x = 0.0f;
		avg_y = 0.0f;
		float avg_z = 0.0f;
		for ( int i = 0; i < cameraPositions.Length; i++ )
		{
			avg_x += cameraPositions[i].x;
			avg_y += cameraPositions[i].y;
			avg_z += cameraPositions[i].z;
		}
		avg_x = avg_x / ((float)cameraPositions.Length);
		avg_y = avg_y / ((float)cameraPositions.Length);
		avg_z = avg_z / ((float)cameraPositions.Length);
		Vector3 avg_vec = new Vector3( avg_x, avg_y, avg_z );

		float avg_size = 0.0f;
		for ( int i = 0; i < cameraSizes.Length; i++ )
		{
			avg_size += cameraSizes[i];
		}
		avg_size = avg_size / ((float)cameraSizes.Length);

		Camera.main.transform.position = avg_vec;
		Camera.main.orthographicSize = avg_size;

		cameraSizeOverwriteIndex = (cameraSizeOverwriteIndex + 1) % cameraSizes.Length;
		cameraPositionOverwriteIndex = (cameraPositionOverwriteIndex + 1) % cameraPositions.Length;
		#endregion
	}
}
