using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class DisableBody : Action
{
	public float _fallSpeed;
	public float _riseSpeed;
	public float _disabledDuration;
	public float _disableTimer;

	private Vector2 _startingPos;
	private Vector2 _groundedPos;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_fallSpeed = 8.0f;
		_riseSpeed = 3.0f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Disabled;

		_disabledDuration = 12.0f;
		_disableTimer = 0.0f;
		_startingPos = (Vector2)transform.position;

		if(_blackboard.body._bodyState == BodyScript.BodyState.OnGound)
		{
			_groundedPos = _startingPos;
		}
		else
		{
			_blackboard.body._bodyState = BodyScript.BodyState.Falling;

			float groundedHeight = Vector2.Distance( _startingPos, _blackboard.body._shadowPos);
			groundedHeight *= _blackboard.body._groundHeightOffset;
			_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);
		}
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		//calculate the height of the boss
		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;

		if(_blackboard.body._bodyState == BodyScript.BodyState.Falling)
		{
			if(Vector2.Distance( (Vector2)transform.position, _groundedPos) > 0.03f)
			{
				transform.position = Vector2.MoveTowards((Vector2)transform.position, _groundedPos, _fallSpeed * (Time.deltaTime* StaticData.t_scale));
			}
			else
			{
				_blackboard.body._bodyState = BodyScript.BodyState.OnGound;
				_blackboard._invincible = false;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.OnGound)
		{
			if(_disableTimer < _disabledDuration)
			{
				_disableTimer += Time.deltaTime * StaticData.t_scale;
			}
			else
			{
				for(int i = 0; i < _blackboard.legsList.Length; i++)
				{
					_blackboard.legsList[i]._currentHP = _blackboard.legsList[i]._maxHP;
					Vector3 Legscale = _blackboard.legsList[i].transform.parent.transform.localScale;
					Legscale.x = 2.25f;
					Legscale.y = 2.25f;
					_blackboard.legsList[i].transform.parent.transform.localScale = Legscale;
					_blackboard.legsList[i]._behaviorState = LegScript.BehaviorState.Walking;
				}
				_blackboard.body._bodyState = BodyScript.BodyState.Recovery;
				_blackboard._invincible = true;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Recovery)
		{
			if(Vector2.Distance( (Vector2)transform.position, _startingPos) > 0.03f)
			{
				transform.position = Vector2.MoveTowards((Vector2)transform.position, _startingPos, _riseSpeed * (Time.deltaTime* StaticData.t_scale));
			}
			else
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Floating;
				return TaskStatus.Success;
			}
		}

		return TaskStatus.Running;
	}

	public override void OnEnd()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Healthy;
		_blackboard.body._bodyState = BodyScript.BodyState.Floating;
	}
}
