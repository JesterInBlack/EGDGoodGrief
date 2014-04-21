using UnityEngine;
using System.Collections;

public class WhiteOut : MonoBehaviour 
{
	private float t;
	private SpriteRenderer mySpriteRenderer;

	// Use this for initialization
	void Start () 
	{
		t = 0.0f;
		mySpriteRenderer = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		// 0.0f -> 1.0f -> 1.0f -> 0.0f
		//  _
		// / \
		t += Time.deltaTime  * StaticData.t_scale;

		float lerpT = Mathf.Min ( 1.0f, t );
		if ( t > 1.0f && t < 2.0f )
		{
			lerpT = 1.0f;
		}
		else if ( t > 2.0f )
		{
			lerpT = Mathf.Max ( 0.0f, 3.0f - t );
		}

		if ( t > 3.0f )
		{
			Destroy ( this.gameObject );
		}

		float r = mySpriteRenderer.color.r;
		float g = mySpriteRenderer.color.g;
		float b = mySpriteRenderer.color.b;
		mySpriteRenderer.color = new Color( r, g, b, lerpT );
	}
}
