using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class AttackWasSuccesful : Action
{

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override TaskStatus OnUpdate()
	{
		_blackboard._attackWasSuccess = true;
		return TaskStatus.Success;
	}
}
