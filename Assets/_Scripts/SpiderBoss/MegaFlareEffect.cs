using UnityEngine;
using System.Collections;

public class MegaFlareEffect : MonoBehaviour 
{
	#region vars
	public GameObject fast;    //set in inspector
	public GameObject medium;  //set in inspector
	public GameObject slow;    //set in inspector

	private const float speed = 16.0f;

	private float t = 0.0f;
	private float delayT = 2.0f;
	private bool passedDelay = false;
	#endregion
	
	// Update is called once per frame
	void Update () 
	{
		float dt = Time.deltaTime * StaticData.t_scale;
		t += dt;
		if ( t < delayT && ! passedDelay )
		{
			return;
		}
		else if ( t >= delayT && ! passedDelay )
		{
			t -= delayT;
			passedDelay = true;
		}

		float lerpT = Mathf.Max ( 0.0f, 1.0f - t / 2.0f );

		fast.transform.position += new Vector3( dt * 1.0f * speed, 0.0f, 0.0f );
		Color color = fast.GetComponent<SpriteRenderer>().color;
		fast.GetComponent<SpriteRenderer>().color = new Color( color.r, color.g, color.b, lerpT );

		//medium.transform.position += new Vector3( dt * 1.0f * speed, 0.0f, 0.0f );
		color = medium.GetComponent<SpriteRenderer>().color;
		medium.GetComponent<SpriteRenderer>().color = new Color( color.r, color.g, color.b, lerpT );

		slow.transform.position += new Vector3( dt * -1.0f * speed, 0.0f, 0.0f );
		color = slow.GetComponent<SpriteRenderer>().color;
		slow.GetComponent<SpriteRenderer>().color = new Color( color.r, color.g, color.b, lerpT );

		if ( lerpT == 0.0f )
		{
			Destroy ( this.gameObject );
		}
	}
}
