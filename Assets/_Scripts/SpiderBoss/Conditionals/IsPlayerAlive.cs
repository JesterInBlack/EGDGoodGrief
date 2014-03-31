using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class IsPlayerAlive : Action
{
	public int _playerNumber;

	public override TaskStatus OnUpdate()
	{
		if(GameState.players[_playerNumber].GetComponent<Player>().HP <= 0)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
