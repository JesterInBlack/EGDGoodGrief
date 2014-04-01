using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireChaseBeam : Action
{
	public float _chaseSpeed = 45.0f;

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
		_eyesScript._behaviorState = EyeScript.BehaviorStates.chaseBeam;
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		_eyesScript.GetTargetAngle(_blackboard.targetPlayer.transform.position);
		_eyesScript.RotateToTarget(_chaseSpeed);
		return TaskStatus.Running;
	}

	public override void OnEnd()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.idle;
	}
}
