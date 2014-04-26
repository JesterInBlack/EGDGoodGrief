using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckSharedDisabledLeg : Action
{
	public SharedInt _legNumber;
	
	private BehaviorBlackboard _blackboard;
	private LegScript _legScript;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

	}

	public override void OnStart()
	{
		_legScript = _blackboard.legsList[_legNumber.Value];
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_legScript._behaviorState != LegScript.BehaviorState.Disabled)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
