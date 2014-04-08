using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class UnityMegaFlareEye : Action
{
	public SharedBool _sharedFinishedAttack;

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
		if(_eyesScript._behaviorState == EyeScript.BehaviorStates.LookDown)
		{
			if(_eyesScript._rotationVec.z == 270 || _eyesScript._rotationVec.z == -90)
			{
				if(_sharedFinishedAttack.Value == false)
				{
					_eyesScript._behaviorState = EyeScript.BehaviorStates.Charge;
				}
			}
		}
		else if(_eyesScript._behaviorState == EyeScript.BehaviorStates.Charge)
		{
			if(_sharedFinishedAttack.Value == true)
			{
				_eyesScript._behaviorState = EyeScript.BehaviorStates.LookDown;
			}
			//TODO give the boss some kinda effect for this
		}
		return TaskStatus.Running;
	}

	public override void OnEnd()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.Idle;
	}
}
