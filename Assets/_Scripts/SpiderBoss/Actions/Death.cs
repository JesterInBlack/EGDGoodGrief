using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class Death : Action
{
	public float _chargeTime;
	public float _chargeDuration;

	private Vector2 _startingPos;

	private Vector2 _shake;
	private float _shakeMagnitude;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_shakeMagnitude = 0.1f;
		_chargeDuration = 4.0f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Dying;
		_blackboard.body._bodyState = BodyScript.BodyState.Charging;
		
		_chargeTime = 0.0f;
		
		_startingPos = (Vector2)transform.position;
		
		_shake = new Vector3( 0.0f, 0.0f, 0.0f );
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(_chargeTime >= _chargeDuration)
			{
				transform.position = _startingPos;
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
				_chargeTime = 0.0f;
			}
			else
			{
				ShakeBoss(_chargeTime / _chargeDuration);
				_chargeTime += Time.deltaTime * StaticData.t_scale;
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
		transform.position = _startingPos + _shake;
	}
}
