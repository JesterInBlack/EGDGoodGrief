using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class ApplyBuff : Action
{
	private BehaviorBlackboard _blackboard;

	public enum BuffType
	{
		venom = 0,
		web = 1,
	}
	public BuffType _buffType;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	//applies the buff to the selected leg
	public override TaskStatus OnUpdate()
	{
		if(_buffType == BuffType.venom)
		{
			Debug.Log("applying venom");
			//have the selected leg move over to the mouth so that it can apply the buff and then return success
			_blackboard.selectedLeg.GetComponent<LegScript>()._buffState = LegScript.BuffState.venom;
			return TaskStatus.Success;
		}
		else if(_buffType == BuffType.web)
		{
			Debug.Log("applying web");
			_blackboard.selectedLeg.GetComponent<LegScript>()._buffState = LegScript.BuffState.web;
			return TaskStatus.Success;
		}

		//no buff type set? return fail
		return TaskStatus.Failure;
	}

}
