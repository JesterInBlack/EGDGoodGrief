using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class SetSharedBool : Action
{
	public SharedBool _checker;
	public bool _setBool;
	
	public override TaskStatus OnUpdate()
	{
		_checker.Value = _setBool;
		return TaskStatus.Success;
	}
}
