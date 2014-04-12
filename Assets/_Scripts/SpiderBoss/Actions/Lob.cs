using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class Lob : Action
{
	public SharedBool _lobbed;

	public float _chargeHeightOffset;
	public float _lobHeightOffset;
	public float _chargeTime;
	public float _lobTime;
	public float _recoveryTime;

	private float _lerpTime;
	private Vector2 _chargePos;
	private Vector2 _startingPos;
	private Vector2 _lobPos;
	private Vector2 _endingPos;
	private Vector2 _groundedPos;
	
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		
		_chargeTime = 1.25f;
		_lobTime = 0.15f;
		_recoveryTime = 1.5f;
		_chargeHeightOffset = -1.0f;
		_lobHeightOffset = 0.5f;
	}

	public override void OnStart()
	{
		_lobbed.Value = false;

		_blackboard.body._behaviorState = BodyScript.BehaviorState.Lob;
		_blackboard.body._bodyState = BodyScript.BodyState.Charging;

		_lerpTime = 0.0f;

		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _chargeHeightOffset);
		_endingPos = (Vector2)_blackboard.body._neutralPoint.transform.position;
		_lobPos = new Vector2(_endingPos.x, _endingPos.y + _lobHeightOffset);

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
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
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _chargeTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Attacking)
		{
			if(Vector2.Distance( (Vector2)transform.position, _lobPos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Recovery;
				_lerpTime = 0.0f;
				_lobbed.Value = true;
			}
			else
			{
				transform.position = Vector2.Lerp(_chargePos, _lobPos, _lerpTime / _lobTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Recovery)
		{
			if(Vector2.Distance( (Vector2)transform.position, _endingPos) < 0.001f)
			{
				return TaskStatus.Success;
			}
			else
			{
				transform.position = Vector2.Lerp(_lobPos, _endingPos, _lerpTime / _recoveryTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		return TaskStatus.Running;
	}

	public override void OnEnd()
	{
		_blackboard.body._bodyState = BodyScript.BodyState.Floating;
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Healthy;
	}
}
