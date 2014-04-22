using UnityEngine;
using System.Collections;

public class BeamWidener : MonoBehaviour 
{
	public GameObject[] particleSystems; //set in inspector.
	private float t = 0.0f;

	// Use this for initialization
	void Start () 
	{
		float tOffset = 0.0f;
		foreach ( GameObject obj in particleSystems )
		{
			obj.renderer.sortingLayerName = "Projectile"; //put beam on proper sorting layer.
			if ( -1.0f * obj.GetComponent<ParticleSystem>().startDelay < tOffset )
			{
				tOffset = -1.0f * obj.GetComponent<ParticleSystem>().startDelay;
			}
		}
		t = tOffset;
	}
	
	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
		if ( t < 0.0f ) { return; }

		float lerpT = Mathf.Min( 1.0f, t / 2.0f );
		float scale = Mathf.Lerp ( 1.0f, 20.0f, lerpT );
		foreach ( GameObject obj in particleSystems )
		{
			obj.transform.localScale = new Vector3( scale, 1.0f, 1.0f );
		}

		for ( float i = -1.0f; i <= 1.0f; i+= 0.5f )
		{
			Vector2 start = new Vector2( transform.position.x, transform.position.y );
			start.x += i * scale * 0.05f * Mathf.Cos ( Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90.0f) );
			start.y += i * scale * 0.05f * Mathf.Sin ( Mathf.Deg2Rad * (transform.rotation.eulerAngles.z + 90.0f) );

			Vector2 end = new Vector2( start.x + 30.0f * Mathf.Cos ( Mathf.Deg2Rad * (transform.rotation.eulerAngles.z ) ), 
			                           start.y + 30.0f * Mathf.Sin ( Mathf.Deg2Rad * (transform.rotation.eulerAngles.z ) ) );
			AttackSystem.hitLineSegment( start, end, 7.5f * Time.deltaTime * StaticData.t_scale, -1 );
		}
	}
}
