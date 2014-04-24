using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class PlayerTargeter : Action
{
	private BehaviorBlackboard _blackboard;

	public enum TargetType
	{
		selection = 0,
		random = 1,
		highestAggro = 2,
		priorityRandom = 3,
	}
	public TargetType _targetType;

	public int _selectionTargetIndex;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_targetType == TargetType.selection)
		{
			_blackboard.targetPlayer = GameState.players[_selectionTargetIndex];
			return TaskStatus.Success;
		}
		else if(_targetType == TargetType.random)
		{
			int randomIndex = Random.Range(0, 4);
			if ( GameState.players[randomIndex] != null )
			{
				if (GameState.players[randomIndex].GetComponent<Player>().isDowned == false)
				{
					_blackboard.targetPlayer = GameState.players[randomIndex];
					return TaskStatus.Success;
				}
			}
		}
		else if(_targetType == TargetType.highestAggro)
		{
			int aggroIndex = AggroTargeter();
			
			if(aggroIndex == -1)
			{
				return TaskStatus.Failure; 
			}
			else
			{
				_blackboard.targetPlayer = GameState.players[aggroIndex];
				return TaskStatus.Success;
			}
		}
		else if(_targetType == TargetType.priorityRandom)
		{
			int choice = Random.Range(0, 3);
			if(choice == 0)
			{
				int randomIndex = Random.Range(0, 4);
				if ( GameState.players[randomIndex] != null )
				{
					if (GameState.players[randomIndex].GetComponent<Player>().isDowned == false)
					{
						_blackboard.targetPlayer = GameState.players[randomIndex];
						return TaskStatus.Success;
					}
				}
			}
			else
			{
				int aggroIndex = AggroTargeter();
				
				if(aggroIndex == -1)
				{
					return TaskStatus.Failure; 
				}
				else
				{
					_blackboard.targetPlayer = GameState.players[aggroIndex];
					return TaskStatus.Success;
				}
			}
		}

		return TaskStatus.Failure;
		
	}

	private int AggroTargeter()
	{
		float highestAggro = 0;
		int aggroIndex = -1;
		for (int i = 0; i < GameState.players.Length; i++)
		{
			if ( GameState.players[i] != null )
			{
				if (GameState.players[i].GetComponent<Player>().isDowned == false && GameState.playerThreats[i] >= highestAggro)
				{
					highestAggro = GameState.playerThreats[i];
					aggroIndex = i;
				}
			}
		}

		return aggroIndex;
	}
}
