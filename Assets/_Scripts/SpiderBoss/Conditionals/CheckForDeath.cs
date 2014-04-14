using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckForDeath : Action
{
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_blackboard.HP <= 0)
		{
			//_blackboard.body._behaviorState = BodyScript.BehaviorState.Dying;
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
