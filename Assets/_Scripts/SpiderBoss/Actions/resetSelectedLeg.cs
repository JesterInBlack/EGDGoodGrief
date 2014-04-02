using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class resetSelectedLeg : Action
{
	private float _lerpTime;
	public float _cooldownTime;


	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_cooldownTime = 2.0f;
	}

	public override void OnStart()
	{
		_lerpTime = 0.0f;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_blackboard.selectedLeg != null)
		{
			if(_lerpTime > _cooldownTime)
			{
				//Debug.Log("It still finishes fine");
				_blackboard.selectedLeg._behaviorState = LegScript.BehaviorState.Walking;
				return TaskStatus.Success;
			}
			else
			{
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
				return TaskStatus.Running;
			}
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
