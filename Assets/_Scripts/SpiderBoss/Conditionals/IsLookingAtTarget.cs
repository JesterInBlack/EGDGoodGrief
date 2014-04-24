using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class IsLookingAtTarget : Action
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
		if(_blackboard.targetPlayer.GetComponent<Player>().isDowned == false)
		{
			if(_blackboard.eye.LookingAtTarget() == true)
			{
				return TaskStatus.Success;
			}
			else
			{
				return TaskStatus.Running;
			}
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
