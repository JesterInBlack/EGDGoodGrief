using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckForDisabledLeg : Action
{
	public int _legNumber;
	
	private BehaviorBlackboard _blackboard;
	private LegScript _legScript;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_legScript = _blackboard.legsList[_legNumber];
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
