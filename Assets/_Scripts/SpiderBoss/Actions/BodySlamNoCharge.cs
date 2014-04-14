﻿using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class BodySlamNoCharge : Action
{
	public float _fallTime;
	public float _riseTime;
	public float _slamDuration;
	public float _slamTimer;

	private float _lerpTime;
	private Vector2 _startingPos;
	private Vector2 _groundedPos;
	private Vector2 _endingPos;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_fallTime = 0.15f;
		_riseTime = 2.0f;
		_slamDuration = 2.5f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.BodySlam;
		_blackboard.body._bodyState = BodyScript.BodyState.Falling;
		
		_slamTimer = 0.0f;
		_lerpTime = 0.0f;

		_startingPos = (Vector2)transform.position;
		_endingPos = (Vector2)_blackboard.body._neutralPoint.transform.position;

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);
	}

	public override TaskStatus OnUpdate ()
	{
		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;

		if(_blackboard.body._bodyState == BodyScript.BodyState.Falling)
		{
			if(Vector2.Distance( (Vector2)transform.position, _groundedPos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.OnGound;
				_lerpTime = 0.0f;
				_blackboard._invincible = false;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _groundedPos, _lerpTime / _fallTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.OnGound)
		{
			if(_slamTimer >= _slamDuration)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Rising;
				_blackboard._invincible = true;
			}
			else
			{
				_slamTimer += Time.deltaTime * StaticData.t_scale;
			}
		}
		else if (_blackboard.body._bodyState == BodyScript.BodyState.Rising)
		{
			if(Vector2.Distance( (Vector2)transform.position, _endingPos) < 0.001f)
			{
				return TaskStatus.Success;
			}
			else
			{
				transform.position = Vector2.Lerp(_groundedPos, _endingPos, _lerpTime / _riseTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}

		return TaskStatus.Running;
	}
}
