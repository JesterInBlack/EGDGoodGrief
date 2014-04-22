using UnityEngine;
using System.Collections;

public class BeamCharge : MonoBehaviour 
{
	private float t;
	private ParticleSystem myParticleSystem;

	private float speed;
	private float lifetime;
	private float rate;

	// Use this for initialization
	void Start () 
	{
		myParticleSystem = GetComponent<ParticleSystem>();
		t = 0.0f;

		speed = myParticleSystem.startSpeed;
		lifetime = myParticleSystem.startLifetime;
		rate = myParticleSystem.emissionRate;
	}
	
	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
		float lerpT = Mathf.Min( 1.0f, t / myParticleSystem.duration );

		myParticleSystem.startSpeed    = Mathf.Lerp ( speed,    speed * 4.0f,    lerpT );
		myParticleSystem.startLifetime = Mathf.Lerp ( lifetime, lifetime / 4.0f, lerpT );
		myParticleSystem.emissionRate  = Mathf.Lerp ( rate,     rate * 4.0f,     lerpT );

		//myParticleSystem.startSpeed    = Mathf.Lerp ( -10.0f, -40.0f,  lerpT );
		//myParticleSystem.startLifetime = Mathf.Lerp (   0.2f,   0.05f, lerpT );
		//myParticleSystem.emissionRate  = Mathf.Lerp ( 100.0f, 400.0f,  lerpT );
	}
}
