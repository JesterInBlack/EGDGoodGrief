using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class CrouchingCharge : Action
{
	public SharedBool _fired;

	public float _chargeTime;
	public float _chargeDuration;

	public float _chargeHeightOffset;
	public float _attackTime;
	public float _recoveryDuration;
	public float _crouchingTime;

	private float _lerpTime;
	private Vector2 _chargePos;
	private Vector2 _startingPos;
	//private Vector2 _endingPos;
	private Vector2 _groundedPos;

	private Vector2 _shake;
	private float _shakeMagnitude;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		
		_chargeHeightOffset = -1.0f;
		_attackTime = 0.50f;
		_recoveryDuration = 0.75f;
		_chargeDuration = 2.0f;
		_crouchingTime = 1.0f;

		_shakeMagnitude = 0.05f;
	}

	public override void OnStart()
	{
		_fired.Value = false;
		
		_blackboard.body._behaviorState = BodyScript.BehaviorState.CrouchingCharge;
		_blackboard.body._bodyState = BodyScript.BodyState.Falling;

		_chargeTime = 0.0f;
		_lerpTime = 0.0f;
		
		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _chargeHeightOffset);
		//_endingPos = (Vector2)_blackboard.body._neutralPoint.transform.position;
		
		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);

		_shake = new Vector3( 0.0f, 0.0f, 0.0f );
	}

	public override TaskStatus OnUpdate ()
	{
		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;

		if(_blackboard.body._bodyState == BodyScript.BodyState.Falling)
		{
			if(Vector2.Distance( (Vector2)transform.position, _chargePos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Charging;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _crouchingTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(_chargeTime >= _chargeDuration)
			{
				//_spawnedMegaFlare.GetComponent<MegaFlareScript>().Attack();
				_fired.Value = true;
				transform.position = _chargePos;
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
				_chargeTime = 0.0f;
			}
			else
			{
				ShakeBoss(_chargeTime / _chargeDuration);
				_chargeTime += Time.deltaTime * StaticData.t_scale;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Attacking)
		{
			if(_chargeTime >= _attackTime)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Recovery;
			}
			else
			{
				_chargeTime += Time.deltaTime * StaticData.t_scale;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Recovery)
		{
			if(Vector2.Distance( (Vector2)transform.position, _startingPos) < 0.001f)
			{
				return TaskStatus.Success;
			}
			else
			{
				transform.position = Vector2.Lerp(_chargePos, _startingPos, _lerpTime / _recoveryDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}

		return TaskStatus.Running;
	}

	//make the boss shake harder over time
	void ShakeBoss(float time)
	{
		float x = Random.Range ( -_shakeMagnitude * time, _shakeMagnitude * time);
		float y = Random.Range ( -_shakeMagnitude * time, _shakeMagnitude * time);
		_shake = new Vector2( x, y );
		transform.position = _chargePos + _shake;
	}

	public override void OnEnd()
	{
		_shake = new Vector2(0, 0);
		_blackboard.body._bodyState = BodyScript.BodyState.Floating;
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Healthy;
	}
}
