using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class BodySlam : Action
{
	public float _bumpHeight;
	public float _chargeTime;
	public float _fallTime;
	public float _riseTime;
	public float _slamDuration;
	public float _slamTimer;

	private float _lerpTime;
	private Vector2 _chargePos;
	private Vector2 _startingPos;
	private Vector2 _groundedPos;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_chargeTime = 3.0f;
		_fallTime = 0.125f;
		_riseTime = 2.0f;
		_slamDuration = 4.0f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.BodySlam;
		_blackboard.body._bodyState = BodyScript.BodyState.Charging;

		_slamTimer = 0.0f;
		_lerpTime = 0.0f;

		_bumpHeight = 1.5f;
		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _bumpHeight);

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= 0.35f;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);
	}
	
	public override TaskStatus OnUpdate ()
	{
		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;

		if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(Vector2.Distance( (Vector2)transform.position, _chargePos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Falling;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _chargeTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Falling)
		{
			if(Vector2.Distance( (Vector2)transform.position, _groundedPos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.OnGound;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _groundedPos, _lerpTime / _fallTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.OnGound)
		{
			if(_slamTimer < _slamDuration)
			{
				_slamTimer += Time.deltaTime * StaticData.t_scale;
			}
			else
			{
				//_blackboard.body._bodyState = BodyScript.BodyState.Rising;
			}
		}
		return TaskStatus.Running;
	}
}
