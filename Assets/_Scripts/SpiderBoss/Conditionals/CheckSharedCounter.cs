using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckSharedCounter : Action 
{
	public SharedInt _counter;
	public SharedInt _minimumCounter;

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_counter.Value >= _minimumCounter.Value)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
