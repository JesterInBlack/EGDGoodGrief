using UnityEngine;
using System.Collections;

public class Lifetime : MonoBehaviour 
{
	#region vars
	public float lifetime = 2.0f; //lifetime in seconds
	private float timer = 0.0f;

	//TODO: GET THIS SHIT THE FUCK OUT OF MY SCRIPT!
	//Make it its own thing. It's not hard to do it right.
	public float stopTime = 5.0f; //when the hit scan should stop
	public bool stopHitting;
	#endregion

	// Use this for initialization
	void Start () 
	{
		stopHitting = false;//TODO: REMOVE
		timer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		timer += Time.deltaTime * StaticData.t_scale;
		//TODO GET RID OF THIS
		if ( timer >= stopTime )
		{
			stopHitting = true;
		}

		if ( timer >= lifetime )
		{
			Destroy ( this.gameObject );
		}
	}
}
