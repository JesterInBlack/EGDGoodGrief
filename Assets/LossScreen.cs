using UnityEngine;
using System.Collections;

public class LossScreen : MonoBehaviour 
{
	#region vars
	public Texture2D bg;
	public Texture2D mainMenuBG;
	public Texture2D pixel;

	private float t = 0.0f;
	private float delay = 4.0f;
	#endregion

	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
	}

	void OnGUI()
	{
		//slide out
		float lerpT = Mathf.Max ( 0.0f, (t - delay) * 0.5f);
		//float lerpT = 0.0f;
		//if (t - delay > 1.0f) { lerpT = 1.0f; }
		GUI.DrawTexture ( new Rect( 0.0f, 0.0f, Screen.width, Screen.height ), mainMenuBG );
		GUI.DrawTexture ( new Rect( 0.0f - Screen.width * lerpT, 0.0f, Screen.width, Screen.height ), bg );

		//lerpT = Mathf.Max ( 0.0f, (t - delay) * 0.5f);
		if ( lerpT > 1.0f )
		{
			Application.LoadLevel ( "MainMenu" );
		}

		//fade in. (done last due to layer order)
		float fadeInT = Mathf.Max ( 0.0f, 1.0f - t );
		GUI.color = new Color( 0.0f, 0.0f, 0.0f, fadeInT );
		GUI.DrawTexture ( new Rect( 0.0f, 0.0f, Screen.width, Screen.height ), pixel );
		GUI.color = Color.white; //reset

		//fade out.
		/*
		float fadeOutT = Mathf.Max ( 0.0f, t - delay );
		if ( t - delay > 1.0f ) { fadeOutT = Mathf.Max ( 0.0f, 1.0f - (t - delay - 1.0f) ); }
		GUI.color = new Color( 1.0f, 1.0f, 1.0f, fadeOutT );
		GUI.DrawTexture ( new Rect( 0.0f, 0.0f, Screen.width, Screen.height ), pixel );
		GUI.color = Color.white; //reset
		*/
	}
}
