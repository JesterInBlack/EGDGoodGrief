using UnityEngine;
using System.Collections;

public class Lifetime : MonoBehaviour 
{
	#region vars
	public float lifetime = 2.0f; //lifetime in seconds
	private float timer = 0.0f;
	#endregion

	// Use this for initialization
	void Start () 
	{
		timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime * StaticData.t_scale;
		if ( timer >= lifetime )
		{
			Destroy ( this.gameObject );
		}
	}
}
