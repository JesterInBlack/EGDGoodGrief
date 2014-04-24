using UnityEngine;
using System.Collections;

public class Lifetime : MonoBehaviour 
{
	#region vars
	public float lifetime = 2.0f; //lifetime in seconds
	public float timer = 0.0f;
	public float stopTime = 5.0f; //when the hit scan should stop
	public bool stopHitting;
	#endregion

	// Use this for initialization
	void Start () 
	{
		stopHitting = false;
		timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime * StaticData.t_scale;
		if(timer >= stopTime)
		{
			stopHitting = true;
		}
		if ( timer >= lifetime )
		{
			Destroy ( this.gameObject );
		}
	}
}
