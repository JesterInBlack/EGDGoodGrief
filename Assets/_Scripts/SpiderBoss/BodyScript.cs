using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyScript : MonoBehaviour 
{

	public float _baseHeight;
	public float _height;
	public bool _invincible;

	[HideInInspector]
	public enum BehaviorState
	{
		Disabled = -1,
		Healthy = 0,
		BodySlam = 1,
	}
	public BehaviorState _behaviorState;

	[HideInInspector]
	public enum BodyState
	{
		Floating = 0,
		Falling = 1,
		OnGound = 2,
		Rising = 3,
		Charging = 4
	}
	public BodyState _bodyState;

	[HideInInspector]
	public Vector2 _shadowPos;

	// Use this for initialization
	void Start () 
	{
		_invincible = true;
		_baseHeight = 3.5f;
		_height = _baseHeight;
		_behaviorState = BehaviorState.Healthy;
		_bodyState = BodyState.Floating;
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
