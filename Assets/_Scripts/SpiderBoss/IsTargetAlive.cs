using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class IsTargetAlive : Action 
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
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;
	}
}
