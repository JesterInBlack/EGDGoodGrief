using UnityEngine;
using System.Collections;

public class EggSpawner : MonoBehaviour 
{
	#region vars
	public GameObject eggPrefab;
	public int count = 60;
	
	public Vector2 ellipsoidR = new Vector2( 2.05f, 2.35f );
	#endregion

	// Use this for initialization
	void Start () 
	{
		GameObject obj;
		for ( int i = 0; i < count; i++ )
		{
			float scale = Random.Range ( 0.75f, 1.125f );
			float angle = Random.Range ( 0.0f, Mathf.PI * 2.0f );
			//compensates for center clustering: even distribution over the area.
			float percentr = Mathf.Sqrt( Random.Range ( 0.0f, 1.0f ) ); 
			float x = this.gameObject.transform.position.x + ellipsoidR.x * percentr * Mathf.Cos ( angle );
			float y = this.gameObject.transform.position.y + ellipsoidR.y * percentr * Mathf.Sin ( angle );
			obj = (GameObject) Instantiate ( eggPrefab, new Vector3( x, y, 0.0f  ), Quaternion.identity );

			obj.transform.parent = this.gameObject.transform;
			obj.transform.localScale = new Vector3( scale, scale, 1.0f );
		}
	}
}
