using UnityEngine;
using System.Collections;

public class VibrationManager : MonoBehaviour 
{
	//This is a class to manage controller vibration signals.

	/*
	So, we need to be able to take and operate on a variety of inputs.
    Deals with the issue that vibration is done by setting a signal that lasts forever, 
    which is overridden by the next set.
    -> need to track + turn off signals
    -> need to deal with combinations of signals.

    Default to off, turn signals off.
	Constant signal for t time.
	Constant signal indefinately ( until we turn it off ).
	  -> could be expressed frame by frame for 1 frame of t?
    Non-constant signal indefinately
      -> could be expressed frame by frame for 1 frame of t?
    Non-constant signal for t time.
       ...parsing / interpolation? (sin waves are out?)
	
	Sum + cap combinations of signals to get your output.
	*/

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//TODO: update values.
	}
}
