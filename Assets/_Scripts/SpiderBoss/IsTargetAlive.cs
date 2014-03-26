using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

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
		if(_blackboard.targetPlayer.GetComponent<Player>().HP > 0.0f)
		{
			return TaskStatus.Running;
		}
		return TaskStatus.Failure;
	}
}
