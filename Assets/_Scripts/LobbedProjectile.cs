using UnityEngine;
using System.Collections;

public class LobbedProjectile : MonoBehaviour 
{
	#region vars
	public int id = 0;            //set when setup.
	public Vector2 aimPoint;      //set when setup.
	public GameObject shadow;     //set in inspector
	public GameObject gasPrefab;  //set in inspector

	private float virtualZ = 0.0f;
	private float virtualY = 0.0f;
	private float virtualX = 0.0f;
	private Vector3 velocityVector;

	private const float gravity = -10.0f; //units / s^2
	private float t = 0.0f;
	#endregion

	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime * StaticData.t_scale;
		shadow.transform.localPosition = new Vector3( 0.0f, -virtualZ, 0.0f );
		velocityVector.z += gravity * Time.deltaTime * StaticData.t_scale;
		virtualZ += velocityVector.z * Time.deltaTime * StaticData.t_scale;
		virtualY += velocityVector.y * Time.deltaTime * StaticData.t_scale;
		virtualX += velocityVector.x * Time.deltaTime * StaticData.t_scale;
		transform.parent.position = new Vector3( virtualX, virtualY + virtualZ, 0.0f );
		transform.Rotate ( 0.0f, 0.0f, 360.0f * 2.5f * Time.deltaTime * StaticData.t_scale );

		if ( virtualZ < 0.0f )
		{
			//TODO: place gas cloud
			//TODO: put gas + glass prefab
			//TODO: pheromone jar hit detection.
			Explode ();
			Destroy ( transform.parent.gameObject );
		}
	}

	public void Fire( Vector2 start, Vector2 end, float velocity )
	{
		float theta = GetAngleOfReach ( start, end, velocity );
		float angle = Mathf.Atan2 ( end.y - start.y, end.x - start.x );
		velocityVector = new Vector3( Mathf.Cos ( theta ) * Mathf.Cos ( angle ) * velocity, 
		                              Mathf.Cos ( theta ) * Mathf.Sin ( angle ) * velocity, 
		                              Mathf.Sin( theta ) * -1.0f * velocity );
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

	private void Explode()
	{
		Instantiate ( gasPrefab, transform.position, Quaternion.identity );
		foreach (Collider2D hit in AttackSystem.getHitsInCircle( aimPoint, 1.0f, id ) )
		{
			Player tempPlayer = hit.gameObject.GetComponent<Player>();
			if ( tempPlayer != null )
			{
				//Max out player threat!
				float maxThreat = 0.0f;
				for ( int i = 0; i < 4; i++ ) { maxThreat = Mathf.Max ( maxThreat, GameState.playerThreats[i] ); }
				GameState.playerThreats[ tempPlayer.id ] = maxThreat + 30.0f;
				
				if ( tempPlayer.id != id )
				{
					//hitting another player with the pheromone jar is not a cooperative move.
					GameState.cooperationAxis = Mathf.Max ( -1.0f,  GameState.cooperationAxis - 0.025f );
				}
			}
		}
	}
}
