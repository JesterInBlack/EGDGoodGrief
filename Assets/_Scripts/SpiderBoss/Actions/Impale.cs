using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class Impale : Action
{
	#region vars
	private BehaviorBlackboard _blackboard;
	private Vector2 _targetPoint;
	private Vector2 _intermediatePoint;
	private Vector2 _startPoint;

	private float _lerpTime;
	public float _windupTime;
	public float _aimTime;
	public float _attackTime;
	public float _cooldownTime;

	public float radius = 1.0f;    //TODO: make < 1
	public float damage = 35.0f;   //TODO: make sane
	#endregion

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
		_cooldownTime = 2.0f;

		damage = 35.0f;
	}

	public override void OnStart()
	{
		_state = AttackState.Windup;
		_blackboard.selectedLeg._state = LegScript.LegState.Idle;
		_blackboard.selectedLeg._behaviorState = LegScript.BehaviorState.Impale;

		_startPoint = (Vector2)_blackboard.selectedLeg.transform.position;
		_targetPoint = (Vector2)_blackboard.targetPlayer.transform.position;
		_intermediatePoint = new Vector2(_blackboard.selectedPoint.transform.position.x, _blackboard.selectedPoint.transform.position.y + 5f);

		_lerpTime = 0.0f;

		//this is for the shadow
		_blackboard.selectedLeg._lerpTime = 0.0f;
		_blackboard.selectedLeg._startPoint = _startPoint;
		_blackboard.selectedLeg._shadowIntermediatePoint = _targetPoint;
		_blackboard.selectedLeg._shadowMoveTime = _windupTime;
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
			}
		}
		else if(_state == AttackState.Aim)
		{
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
				//Debug.Log("Start the attack!");
				_state = AttackState.Cooldown;
				_lerpTime = 0.0f;

				//THIS IS THE MOMENT WHERE THE ATTACK DAMAGE IS DEALT TO THE TARGET AREA!
				//USE THE ATTACHED CIRCLE COLLIDER
				//Not until you give me a good way to access it.
				AttackSystem.hitCircle ( _targetPoint, radius, damage, -1 );
				GameState.cameraController.Shake ( 0.1f, 0.25f );

				//Debug.Log("does it ever reach here?");

				#region poison
				//If poisonous, go through all hit objects, find the players, and poison them.
				if ( _blackboard.selectedLeg.GetComponent<LegScript>().isPoisonous() )
				{
					foreach ( Collider2D hit in AttackSystem.getHitsInCircle( _targetPoint, radius, -1) )
					{
						Player tempPlayer = hit.collider.gameObject.GetComponent<Player>();
						if ( tempPlayer != null ) //this is a player.
						{
							LegScript tempLeg = _blackboard.selectedLeg.GetComponent<LegScript>();
							tempPlayer.Poison ( tempLeg._poisonDuration, tempLeg._poisonDPS );
						}
					}
				}
				#endregion
			}
			else
			{
				//Debug.Log("winding up");
				_blackboard.selectedLeg.transform.position = Vector2.Lerp(_startPoint, _targetPoint, _lerpTime / _attackTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Cooldown)
		{
			if(_lerpTime > _cooldownTime)
			{

				//Debug.Log("It still finishes fine");
				_blackboard.selectedLeg._behaviorState = LegScript.BehaviorState.Walking;
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
