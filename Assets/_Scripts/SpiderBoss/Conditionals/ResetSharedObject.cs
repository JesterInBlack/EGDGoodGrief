using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class ResetSharedObject : Action
{
	public SharedGameObject _sharedObject;

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		_sharedObject.Value = null;
		return TaskStatus.Failure;
	}
}
