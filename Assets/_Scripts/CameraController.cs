using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour 
{
	//this class tracks the position of the players, and "zooms" the camera in and out to keep them all onscreen.


	#region vars	
	public float min_orthographic_size = 4.0f;  //"maximum zoom factor"
	public float base_orthographic_size = 0.5f; //extra spacing: forces the camera to zoom out a bit.

	private Vector3 shake; //used for camera shake
	private float shakeT;
	private float shakeMagnitude;

	//"looped" arrays for smoothing using simple filtering:
	//[a], [b], [c]
	// ^ replace
	//[1], [b], [c]
	//      ^ replace
	//[1], [2], [c]
	//           ^ replace
	//repeat
	private Vector3[] cameraPositions = new Vector3[10];
	private int cameraPositionOverwriteIndex = 0;
	private float[] cameraSizes = new float[10];
	private int cameraSizeOverwriteIndex = 0;

	//define clamp region.
	private bool clamp = false;
	private float maxX = 20.0f;
	private float minX = -20.0f;
	private float maxY = 12.0f;
	private float minY = -12.0f;
	private float max_orthographic_size = 12.0f; //furthest out you can zoom
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

		shake = new Vector3( 0.0f, 0.0f, 0.0f );
	}
	
	// Update is called once per frame
	void Update () 
	{
		FollowUpdate (); //update camera position factor to follow avg. player position
		ZoomUpdate ();   //update camera zoom factor to keep all players onscreen
		ShakeUpdate ();  //camera shake effects
		Filter ();       //smooth changes over time
	}

	void FollowUpdate ()
	{
		#region follow
		//make it follow the avg. position of the players.
		float avg_x = 0.0f;
		float avg_y = 0.0f;
		float playercount = 0.0f;
		
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i] != null )
			{
				avg_x += GameState.players[i].transform.position.x;
				avg_y += GameState.players[i].transform.position.y;
				playercount ++;
			}
		}
		//throw boss into the avg.
		if ( GameState.boss != null )
		{
			avg_x += GameState.boss.transform.position.x;
			avg_y += GameState.boss.transform.position.y;
			playercount ++;
		}
		avg_x = avg_x / playercount;
		avg_y = avg_y / playercount;
		
		cameraPositions[ cameraPositionOverwriteIndex ] = new Vector3( avg_x, avg_y, -10.0f );
		#endregion
	}

	void ZoomUpdate ()
	{
		#region zoom
		//fit to max dist in y from center, max dist in x from center. Floor on zoom in.
		//float ratio = 1.67f; //factor to divide x by (aspect ratio)
		float ratio = Camera.main.aspect;
		//ratio = (float)Screen.width / (float)Screen.height; //same as ^
		//Debug.Log ( ratio );
		
		float max_dist_x = 0.0f;
		float max_dist_y = 0.0f;
		
		for ( int i = 0; i < GameState.players.Length; i++ )
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
		//throw boss in.
		if ( GameState.boss != null )
		{
			max_dist_x = Mathf.Max ( 
			                        Mathf.Abs( Camera.main.transform.position.x - GameState.boss.transform.position.x ) / ratio, 
			                        max_dist_x 
			                        );
			max_dist_y = Mathf.Max ( 
			                        Mathf.Abs( Camera.main.transform.position.y - GameState.boss.transform.position.y ), 
			                        max_dist_y 
			                        );
		}
		float max_dist = Mathf.Max ( max_dist_x, max_dist_y );
		
		cameraSizes[ cameraSizeOverwriteIndex ] = Mathf.Max ( min_orthographic_size, max_dist + base_orthographic_size ); 
		//captures an ellipse: 1.0 in the y, 1.67 in the x
		#endregion
	}

	void Filter()
	{
		#region filter
		//averages the camera's target position over a few frames for a smoothing effect.
		float avg_x = 0.0f;
		float avg_y = 0.0f;
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

		#region clamp
		if ( clamp )
		{
			float ratio = Camera.main.aspect;
			Debug.Log ( ratio );
			if ( avg_size > max_orthographic_size ) { avg_size = max_orthographic_size; }
			if ( (avg_vec.x + avg_size) * ratio > maxX )
			{
				avg_vec.x = maxX - avg_size * ratio;
			}
			if ( (avg_vec.x - avg_size) * ratio < minX )
			{
				avg_vec.x = minX + avg_size * ratio;
			}
			if ( avg_vec.y + avg_size > maxY )
			{
				avg_vec.y = maxY - avg_size;
			}
			if ( avg_vec.y - avg_size < minY )
			{
				avg_vec.y = minY + avg_size;
			}
		}
		#endregion

		Camera.main.transform.position = avg_vec + shake;
		Camera.main.orthographicSize = avg_size;
		
		cameraSizeOverwriteIndex = (cameraSizeOverwriteIndex + 1) % cameraSizes.Length;
		cameraPositionOverwriteIndex = (cameraPositionOverwriteIndex + 1) % cameraPositions.Length;
		#endregion
	}

	void ShakeUpdate()
	{
		#region shake
		//updates camera shake / zeroes it out.
		if ( shakeT <= 0.0f ) { return; } //no shake.

		float dt = Time.deltaTime * StaticData.t_scale;
		shakeT -= dt;
		if ( shakeT > 0.0f )
		{
			float x = Random.Range ( -shakeMagnitude, shakeMagnitude ) * Camera.main.aspect;
			float y = Random.Range ( -shakeMagnitude, shakeMagnitude );
			shake = new Vector3( x, y, 0.0f ); //TODO: shake based on t.
		}
		else
		{
			shake = new Vector3( 0.0f, 0.0f, 0.0f); //zero out
		}
		#endregion
	}

	public void Shake( float magnitude, float t )
	{
		//shakes the camera
		shakeMagnitude = magnitude * ( Camera.main.orthographicSize / min_orthographic_size );
		shakeT = t;
	}
}
