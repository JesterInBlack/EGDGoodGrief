using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckSharedFloat : Action
{

	public SharedFloat _checker;
	public float _minValue;
	
	public override TaskStatus OnUpdate()
	{
		if(_checker.Value >= _minValue)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
