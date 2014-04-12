using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class ChasePlayer : Action 
{
	public float _speed;

	public enum BehaviorType
	{
		ChaseTarget = 0,
		ChaseHighestAggro = 1,
	}
	public BehaviorType _behaviorType;

	private GameObject _aggroTarget;
	private Player _targetScript;
	private BehaviorBlackboard _blackboard;
	private bool _everyoneDown;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_speed = 1.0f;
		_everyoneDown = false;
	}

	public override void OnStart()
	{

		if(_behaviorType == BehaviorType.ChaseTarget)
		{
			_targetScript = _blackboard.targetPlayer.GetComponent<Player>();
		}
		else if(_behaviorType == BehaviorType.ChaseHighestAggro)
		{
			TargetHighestAggro();
			_targetScript = _aggroTarget.GetComponent<Player>();
		}
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_everyoneDown == false)
		{
			if(_behaviorType == BehaviorType.ChaseTarget)
			{
				if(_targetScript.isDowned == true)
				{
					_everyoneDown = true;
				}
				else
				{
					Vector2 offsetPos = (Vector2)_blackboard.targetPlayer.transform.position;
					offsetPos.y += _blackboard.body._height;
					// We haven't reached the target yet so keep moving towards it
					transform.position = Vector2.MoveTowards((Vector2)transform.position, offsetPos, _speed * (Time.deltaTime* StaticData.t_scale));

					//Vector2 direction = (Vector2)_blackboard.targetPlayer.transform.position - (Vector2)transform.position;
					Vector2 direction = (Vector2)_blackboard.targetPlayer.transform.position - _blackboard.body._shadowPos;
					direction.Normalize();
					_blackboard.moveDirection = direction;
				}

			}
			else if(_behaviorType == BehaviorType.ChaseHighestAggro)
			{
				if(_targetScript.isDowned == true)
				{
					TargetHighestAggro();
				}
				else
				{
					Vector2 offsetPos = (Vector2)_aggroTarget.transform.position;
					offsetPos.y += _blackboard.body._height;
					// We haven't reached the target yet so keep moving towards it
					transform.position = Vector2.MoveTowards((Vector2)transform.position, offsetPos, _speed * (Time.deltaTime * StaticData.t_scale));
					
					//Vector2 direction = (Vector2)_blackboard.targetPlayer.transform.position - (Vector2)transform.position;
					Vector2 direction = (Vector2)_aggroTarget.transform.position - _blackboard.body._shadowPos;
					direction.Normalize();
					_blackboard.moveDirection = direction;
				}
			}
			return TaskStatus.Running; 
		}
		return TaskStatus.Success;
	}

	void TargetHighestAggro()
	{
		float highestAggro = 0;
		int aggroIndex = -1;
		for(int i = 0; i < GameState.players.Length; i++)
		{
			if(GameState.players[i].GetComponent<Player>().isDowned == false && GameState.playerThreats[i] >= highestAggro)
			{
				highestAggro = GameState.playerThreats[i];
				aggroIndex = i;
			}
		}
		
		if(aggroIndex == -1)
		{
			_everyoneDown = true;
		}
		else
		{
			_aggroTarget = GameState.players[aggroIndex];
		}
	}

	public override void OnEnd ()
	{
		_blackboard.moveDirection = new Vector2(0,0);
	}
}
