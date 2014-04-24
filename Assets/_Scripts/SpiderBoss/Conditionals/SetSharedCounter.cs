using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class SetSharedCounter : Action
{
	public bool _increment;
	public SharedInt _counter;
	public int _counterSetValue = 0;
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_increment == true)
		{
			_counter.Value++;
		}
		else
		{
			_counter.Value = _counterSetValue;
		}
		return TaskStatus.Success;
	}
}
