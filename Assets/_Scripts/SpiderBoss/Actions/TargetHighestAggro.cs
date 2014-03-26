using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class TargetHighestAggro: Action 
{
	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		float highestAggro = 0;
		int aggroIndex = -1;
		for(int i = 0; i < GameState.players.Length; i++)
		{
			if(GameState.players[i].GetComponent<Player>().HP > 0 && GameState.playerThreats[i] >= highestAggro)
			{
				highestAggro = GameState.playerThreats[i];
				aggroIndex = i;
			}
		}

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
