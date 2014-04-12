using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckSharedBool : Action
{
	public SharedBool _checker;
	
	public override TaskStatus OnUpdate()
	{
		if(_checker.Value == false)
		{
			return TaskStatus.Failure;
		}
		else
		{
			return TaskStatus.Success;
		}
	}
}
