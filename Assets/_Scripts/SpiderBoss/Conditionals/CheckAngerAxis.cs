using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckAngerAxis : Action
{
	public float _minimumAngerValue;

	public override TaskStatus OnUpdate()
	{
		if(GameState.angerAxis > _minimumAngerValue)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}

	}
}
