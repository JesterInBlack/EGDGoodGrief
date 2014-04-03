using UnityEngine;
using System.Collections;

public class BulletTimeManager : MonoBehaviour 
{
	// A script to make bullet time end.
	// Messes with StaticData. Is the only class that should be allowed to do that every frame.
	// Update is called once per frame
	public Texture2D clockOverlay; //set in inspector

	void Update () 
	{
		StaticData.bulletTimeDuration = Mathf.Max ( 0.0f, StaticData.bulletTimeDuration - Time.deltaTime );
		if ( StaticData.t_scale != 1.0f && StaticData.bulletTimeDuration <= 0.0f )
		{
			StaticData.t_scale = 1.0f;
		}
	}

	void OnGUI()
	{
		if ( StaticData.t_scale != 1.0f )
		{
			float percent = StaticData.bulletTimeDuration / 12.0f; //TODO: rip duration from something else.
			GUI.color = new Color( 0.0f, 0.0f, 1.0f, 0.10f * percent + 0.10f );
			float angle = -360.0f * percent;
			GUIUtility.RotateAroundPivot ( angle, new Vector2( (Screen.width  - 900.0f) / 2.0f + 450.0f, 
			                                                   (Screen.height - 900.0f) / 2.0f + 450.0f ) );
			GUI.DrawTexture ( new Rect( (Screen.width - 900.0f) / 2.0f, (Screen.height - 900.0f) / 2.0f, 900.0f, 900.0f ), clockOverlay );
			GUI.color = Color.white;
		}
	}
}
