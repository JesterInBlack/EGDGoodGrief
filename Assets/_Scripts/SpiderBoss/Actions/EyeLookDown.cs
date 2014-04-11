using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class EyeLookDown : Action
{
	private EyeScript _eyesScript;
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_eyesScript = _blackboard.eye.GetComponent<EyeScript>();
	}
	
	public override void OnStart()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.LookDown;
	}
	
	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Running;
	}
	
	public override void OnEnd()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.Idle;
	}
}
