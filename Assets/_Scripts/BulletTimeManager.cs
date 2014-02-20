using UnityEngine;
using System.Collections;

public class BulletTimeManager : MonoBehaviour 
{
	// A script to make bullet time end.
	// Messes with StaticData. Is the only class that should be allowed to do that every frame.
	// Update is called once per frame
	void Update () 
	{
		StaticData.bulletTimeDuration = Mathf.Max ( 0.0f, StaticData.bulletTimeDuration - Time.deltaTime );
		if ( StaticData.t_scale != 1.0f && StaticData.bulletTimeDuration <= 0.0f )
		{
			StaticData.t_scale = 1.0f;
		}
	}
}
