using UnityEngine;
using System.Collections;

public class Oscillate : MonoBehaviour 
{
	private float t;

	// Use this for initialization
	void Start () 
	{
		t = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
		if ( t > Mathf.PI * 2.0f )
		{
			t -= ( Mathf.PI * 2.0f );
		}
		transform.localPosition = new Vector3( 0.0f, 0.25f * Mathf.Sin ( t ), -1.0f );
	}
}
