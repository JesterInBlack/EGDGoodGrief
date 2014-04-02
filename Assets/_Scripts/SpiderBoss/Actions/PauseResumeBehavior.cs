using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class PauseResumeBehavior : Action
{
	[HideInInspector]
	public enum Pause
	{
		PauseBehavior = 0,
		ResumeBehavior = 1,
	}
	public Pause _pause;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override TaskStatus OnUpdate()
	{
		if(_pause == Pause.PauseBehavior)
		{
			_blackboard._currentBehavior.Action.disableBehavior(true);
		}
		else if(_pause == Pause.ResumeBehavior)
		{
			_blackboard._currentBehavior.Action.enableBehavior();
		}
		return TaskStatus.Success;
	}
}
