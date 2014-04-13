using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckSharedGameObject : Action
{
	public SharedGameObject _checker;
	
	public override TaskStatus OnUpdate()
	{
		if(_checker.Value == null)
		{
			return TaskStatus.Failure;
		}
		else
		{
			return TaskStatus.Success;
		}
	}
}
