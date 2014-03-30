using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class Impale : Action
{
	private BehaviorBlackboard _blackboard;
	private Vector2 _targetPoint;
	private Vector2 _intermediatePoint;
	private Vector2 _startPoint;

	private float _lerpTime;
	public float _windupTime;
	public float _aimTime;
	public float _attackTime;
	public float _cooldownTime;
	
	private enum AttackState
	{
		Windup = 0,
		Aim = 1,
		Attack = 2,
		Cooldown = 3,
	}
	private AttackState _state;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_windupTime = 0.4f;
		_aimTime = 1.5f;
		_attackTime = 0.1f;
		_cooldownTime = 3.0f;
	}

	public override void OnStart()
	{
		_state = AttackState.Windup;
		_blackboard.selectedLeg.GetComponent<LegScript>()._state = LegScript.LegState.Idle;
		_blackboard.selectedLeg.GetComponent<LegScript>()._behaviorState = LegScript.BehaviorState.Impale;

		_startPoint = (Vector2)_blackboard.selectedLeg.transform.position;
		_targetPoint = (Vector2)_blackboard.targetPlayer.transform.position;
		_intermediatePoint = new Vector2(_blackboard.selectedPoint.transform.position.x, _blackboard.selectedPoint.transform.position.y + 5f);

		_lerpTime = 0.0f;

		//this is for the shadow
		_blackboard.selectedLeg.GetComponent<LegScript>()._lerpTime = 0.0f;
		_blackboard.selectedLeg.GetComponent<LegScript>()._startPoint = _startPoint;
		_blackboard.selectedLeg.GetComponent<LegScript>()._shadowIntermediatePoint = _targetPoint;
		_blackboard.selectedLeg.GetComponent<LegScript>()._shadowMoveTime = _windupTime;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_state == AttackState.Windup)
		{
			if( Vector2.Distance((Vector2)_blackboard.selectedLeg.transform.position, _intermediatePoint) < 0.01f)
			{
				_state = AttackState.Aim;
				_lerpTime = 0.0f;
				_startPoint = (Vector2)_blackboard.selectedLeg.transform.position;
				_intermediatePoint = Vector2.Lerp(_startPoint, (Vector2)_blackboard.targetPlayer.transform.position, 0.15f);
			}
			else
			{
				_blackboard.selectedLeg.transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _windupTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
				//transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
			}
		}
		else if(_state == AttackState.Aim)
		{
			/*
			if( Vector2.Distance((Vector2)_blackboard.selectedLeg.transform.position, _intermediatePoint) < 0.001f)
			{
				_state = AttackState.Attack;
				_lerpTime = 0.0f;
				_startPoint = (Vector2)_blackboard.selectedLeg.transform.position;
				_targetPoint = (Vector2)_blackboard.targetPlayer.transform.position;
			}
			else
			{
				_intermediatePoint = Vector2.Lerp(_startPoint, (Vector2)_blackboard.targetPlayer.transform.position, 0.1f);
				_blackboard.selectedLeg.transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _aimTime);
				_lerpTime += Time.deltaTime;
			}
			*/
			if( Vector2.Distance((Vector2)_blackboard.selectedLeg.transform.position, _intermediatePoint) < 0.001f)
			{
				_state = AttackState.Attack;
				_lerpTime = 0.0f;
				_startPoint = (Vector2)_blackboard.selectedLeg.transform.position;
			}
			else
			{
				_blackboard.selectedLeg.transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _aimTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Attack)
		{
			if( Vector2.Distance((Vector2)_blackboard.selectedLeg.transform.position, _targetPoint) < 0.005f)
			{
				_state = AttackState.Cooldown;
				_lerpTime = 0.0f;

				//THIS IS THE MOMENT WHERE THE ATTACK DAMAGE IS DEALT TO THE TARGET AREA!
				//USE THE ATTACHED CIRCLE COLLIDER
			}
			else
			{
				_blackboard.selectedLeg.transform.position = Vector2.Lerp(_startPoint, _targetPoint, _lerpTime / _attackTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Cooldown)
		{
			if(_lerpTime > _cooldownTime)
			{
				_blackboard.selectedLeg.GetComponent<LegScript>()._behaviorState = LegScript.BehaviorState.Walking;
				return TaskStatus.Success;
			}
			else
			{
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}

		return TaskStatus.Running;
	}
}
