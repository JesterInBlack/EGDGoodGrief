using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class IsPlayerAlive : Action
{
	public int _playerNumber;

	public override TaskStatus OnUpdate()
	{
		if(GameState.players[_playerNumber].GetComponent<Player>().isDowned == false)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
