using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public class ScheduledVibration
{
	public float right;
	public float left;
	public float t;
	public ScheduledVibration( float signalLeft, float signalRight, float duration )
	{
		left = signalLeft;
		right = signalRight;
		t = duration;
	}
}

public class VibrationManager : MonoBehaviour 
{
	//This is a class to manage controller vibration signals.
	//"this frame" signals: takes the largest, and plays it
	//"for t" : constant signals, combined with "this frame" signals
	//implicitly handles resetting vibration to 0.

	#region vars
	private PlayerIndex playerIndex;   //controller id, rip from player data.
	private float thisFrameL = 0.0f;   //signal strength (for this frame) (1 source)
	private float thisFrameR = 0.0f;   //signal strength (for this frame) (1 source)

	private ArrayList vibrations = new ArrayList();
	private float accumulatorL = 0.0f; //signal strength accumulator for vibration list.
	private float accumulatorR = 0.0f; //signal strength accumulator for vibration list.
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		playerIndex = GetComponent<CustomController>().playerIndex;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region timer
		float dt = Time.deltaTime;
		if ( GetComponent<Player>().isInBulletTime == false )
		{
			dt = dt * StaticData.t_scale;
		}
		#endregion

		//accumulate all signals.
		accumulatorL = 0.0f;
		accumulatorR = 0.0f;
		for ( int i = 0; i < vibrations.Count; i++ )
		{
			accumulatorL += ((ScheduledVibration) vibrations[i]).left * StaticData.t_scale;
			accumulatorR += ((ScheduledVibration) vibrations[i]).right * StaticData.t_scale;
			((ScheduledVibration) vibrations[i]).t -= dt;
		}
		//delete expired vibrations
		for ( int i = vibrations.Count - 1; i >= 0; i-- )
		{
			if ( ((ScheduledVibration) vibrations[i]).t <= 0.0f )
			{
				vibrations.RemoveAt ( i );
			}
		}
	}

	void LateUpdate()
	{
		float signalL = Mathf.Min ( 1.0f, accumulatorL + thisFrameL ); 
		float signalR = Mathf.Min ( 1.0f, accumulatorR + thisFrameR );
		GamePad.SetVibration ( playerIndex, signalL, signalR );
		thisFrameL = 0.0f;
		thisFrameR = 0.0f;
	}

	public void AddVibrationForThisFrame( float signalL, float signalR )
	{
		//Add a vibration for this frame only. (use w/ continuous setters)
		if ( thisFrameR == 0.0f )
		{
			thisFrameR = signalR;
		}
		else
		{
			thisFrameR = Mathf.Max ( thisFrameR, signalR );
		}

		if ( thisFrameL == 0.0f )
		{
			thisFrameL = signalL;
		}
		else
		{
			thisFrameL = Mathf.Max ( thisFrameL, signalL );
		}
	}

	public void ScheduleVibration( float signalLeft, float signalRight, float duration )
	{
		//Adds a vibration for duration.
		ScheduledVibration vibration = new ScheduledVibration( signalLeft, signalRight, duration );
		vibrations.Add ( vibration );
	}
}
