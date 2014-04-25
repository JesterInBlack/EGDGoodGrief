using UnityEngine;
using System.Collections;

public class StonePunch : MonoBehaviour 
{
	//a class used by the monk for the ranged punch attack.
	#region vars
	public float angle;
	public float range;

	private float speed;
	#endregion

	// Use this for initialization
	void Start () 
	{
		speed = range / GetComponent<Lifetime>().lifetime;
		transform.Rotate ( new Vector3( 0.0f, 0.0f, 180.0f + angle * Mathf.Rad2Deg ) ); //make it shoot in the correct direction
	}
	
	// Update is called once per frame
	void Update () 
	{
		float dt = Time.deltaTime * StaticData.t_scale;
		transform.position += new Vector3( speed * Mathf.Cos ( angle ) * dt, speed * Mathf.Sin ( angle ) * dt, 0.0f );
	}
}
