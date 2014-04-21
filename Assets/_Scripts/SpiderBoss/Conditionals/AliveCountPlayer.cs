using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class AliveCountPlayer : Action
{
	public int _playerCountThreshold;

	public override TaskStatus OnUpdate()
	{
		int aliveCount = 0;
		for (int i = 0; i < GameState.players.Length; i++)
		{
			if ( GameState.players[i] != null )
			{
				if (GameState.players[i].GetComponent<Player>().isDowned == false)
				{
					aliveCount++;
				}
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
