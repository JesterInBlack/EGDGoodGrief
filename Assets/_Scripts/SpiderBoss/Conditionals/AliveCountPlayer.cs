using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class AliveCountPlayer : Action
{
	private BehaviorBlackboard _blackboard;
	
	public int _playerCountThreshold;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override TaskStatus OnUpdate()
	{
		int aliveCount = 0;
		for(int i = 0; i < GameState.players.Length; i++)
		{
			if(GameState.players[i].GetComponent<Player>().HP > 0)
			{
				aliveCount++;
			}
		}

		if(aliveCount >= _playerCountThreshold)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
