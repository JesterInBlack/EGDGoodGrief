using UnityEngine;
using System.Collections;

public class LobbedProjectile : MonoBehaviour 
{
	#region vars
	private float virtualZ = 0.0f;
	private float virtualY = 0.0f;
	private float virtualX = 0.0f;
	private Vector3 velocityVector;

	private const float gravity = -10.0f; //units / s^2
	#endregion

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		velocityVector.z += gravity * Time.deltaTime * StaticData.t_scale;
		virtualZ += velocityVector.z * Time.deltaTime * StaticData.t_scale;
		virtualY += velocityVector.y * Time.deltaTime * StaticData.t_scale;
		virtualX += velocityVector.x * Time.deltaTime * StaticData.t_scale;
		transform.position = new Vector3( virtualX, virtualY + virtualZ, 0.0f );

		if ( virtualZ < 0.0f )
		{
			//?
		}
	}

	public void Fire( Vector2 start, Vector2 end, float velocity )
	{
		float theta = GetAngleOfReach ( start, end, velocity );
		velocityVector = new Vector3( Mathf.Cos ( theta ), Mathf.Sin ( theta ), 0.0f );
		transform.position = new Vector3( start.x, start.y, 0.0f );
		virtualX = start.x;
		virtualY = start.y;
		virtualZ = 0.0f;
	}

	public float GetAngleOfReach( Vector2 start, Vector2 end, float velocity )
	{
		//45 degrees: max range
		float dist = (end - start).magnitude;
		return 0.5f * Mathf.Asin ( gravity * dist / (velocity * velocity) );
	}
}
