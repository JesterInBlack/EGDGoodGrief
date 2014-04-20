using UnityEngine;
using System.Collections;

public class BeamCharge : MonoBehaviour 
{
	private float t;
	private ParticleSystem particleSystem;

	// Use this for initialization
	void Start () 
	{
		particleSystem = GetComponent<ParticleSystem>();
		t = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
		float lerpT = Mathf.Min( 1.0f, t / GetComponent<ParticleSystem>().duration );

		particleSystem.startSpeed = Mathf.Lerp ( -10.0f, -40.0f, lerpT );
		particleSystem.startLifetime = Mathf.Lerp ( 0.2f, 0.05f, lerpT );
		particleSystem.emissionRate = Mathf.Lerp ( 100.0f, 400.0f, lerpT );
	}
}
