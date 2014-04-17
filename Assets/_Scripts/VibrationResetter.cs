using UnityEngine;
using System.Collections;
using XInputDotNetPure; //Required in C#

public class VibrationResetter : MonoBehaviour 
{
	//Reset vibration on entering the scene.

	// Use this for initialization
	void Start () 
	{
		GamePad.SetVibration ( PlayerIndex.One,   0.0f, 0.0f );
		GamePad.SetVibration ( PlayerIndex.Two,   0.0f, 0.0f );
		GamePad.SetVibration ( PlayerIndex.Three, 0.0f, 0.0f );
		GamePad.SetVibration ( PlayerIndex.Four,  0.0f, 0.0f );
	}
}
