using UnityEngine;
using System.Collections;

public class BulletTimeManager : MonoBehaviour 
{
	// A script to make bullet time end.
	// Messes with StaticData. Is the only class that should be allowed to do that every frame.
	// Update is called once per frame

	//TODO: make bullet time fade in / out. (1s in, 1s out)
	public Texture2D clockOverlay; //set in inspector

	public static float temporalScalingFactor = 0.5f;
	private float lerpT = 0.0f;
	private bool fadein = false;
	private bool fadeout = false;

	void Update () 
	{
		//TODO: if bullet time duration > 1, t scale != 0.5f, lerpto it in 1 sec, play sound.
		//TODO: if bullet time duration <= 1.0f, lerpto 1.0f in 1 sec, play sound.
		//redundant fade ins don't trip the sound or the time scale lerp.

		StaticData.bulletTimeDuration = Mathf.Max ( 0.0f, StaticData.bulletTimeDuration - Time.deltaTime * StaticData.t_scale );
		if ( StaticData.bulletTimeDuration > 1.0f && StaticData.t_scale > temporalScalingFactor ) //fade in.
		{
			lerpT += Time.deltaTime * StaticData.t_scale;
			StaticData.t_scale = Mathf.Lerp ( 1.0f, temporalScalingFactor, Mathf.Min ( 1.0f, lerpT ) );
			if ( ! fadein ) { GetComponent<AudioSource>().PlayOneShot ( SoundStorage.ItemSlowMoIn, 1.0f ); }
			//state?
			fadein = true;
			fadeout = false;
		}
		else if ( StaticData.bulletTimeDuration < 1.0f && StaticData.t_scale < 1.0f ) //fade out.
		{
			float tempLerpT = 1.0f - StaticData.bulletTimeDuration; //0 to 1
			StaticData.t_scale = Mathf.Lerp ( temporalScalingFactor, 1.0f, Mathf.Min ( 1.0f, tempLerpT ) );
			if ( ! fadeout ) { GetComponent<AudioSource>().PlayOneShot ( SoundStorage.ItemSlowMoOut, 1.0f ); }
			//state?
			fadeout = true;
			fadein = false;
			lerpT = 0.0f; //reset so fade in can use it.
		}

		if ( StaticData.bulletTimeDuration <= 0.0f ) //cut out
		{
			if ( StaticData.t_scale != 1.0f )
			{
				StaticData.t_scale = 1.0f;
			}
			//state?
			fadein = false;
			fadeout = false;
			lerpT = 0.0f;
		}
	}

	void OnGUI()
	{
		if ( StaticData.t_scale != 1.0f )
		{
			float percent = StaticData.bulletTimeDuration / (1.0f + 3.0f * temporalScalingFactor); //TODO: rip duration from something else.
			GUI.color = new Color( 0.0f, 0.0f, 1.0f, 0.10f * percent + 0.10f );
			float angle = -360.0f * percent;
			GUIUtility.RotateAroundPivot ( angle, new Vector2( (Screen.width  - 900.0f) / 2.0f + 450.0f, 
			                                                   (Screen.height - 900.0f) / 2.0f + 450.0f ) );
			GUI.DrawTexture ( new Rect( (Screen.width - 900.0f) / 2.0f, (Screen.height - 900.0f) / 2.0f, 900.0f, 900.0f ), clockOverlay );
			GUI.color = Color.white;
		}
	}
}
