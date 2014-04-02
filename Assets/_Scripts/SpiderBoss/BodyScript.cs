using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyScript : MonoBehaviour 
{

	public float _height;

	[HideInInspector]
	public enum BehaviorState
	{
		Disabled = -1,
		Healthy = 0,
	}
	public BehaviorState _behaviorState;

	[HideInInspector]
	public enum BodyState
	{
		floating = 0,
		falling = 1,
		onGound = 2,
		rising = 3,
	}
	public BodyState _bodyState;

	[HideInInspector]
	public Vector2 _shadowPos;

	// Use this for initialization
	void Start () 
	{
		_height = 3.5f;
		_behaviorState = BehaviorState.Healthy;
		_bodyState = BodyState.floating;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_behaviorState == BehaviorState.Healthy)
		{
			_shadowPos = (Vector2)transform.position;
			_shadowPos.y -= _height;
		}

	}
}
