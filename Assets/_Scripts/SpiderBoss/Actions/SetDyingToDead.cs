using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class SetDyingToDead : Action
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
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Dead;
		_blackboard._moveToEndScreen = true;
		return TaskStatus.Success;
	}

}
