using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class ApplyBuff : Action
{
	private BehaviorBlackboard _blackboard;

	public GameObject rightLegsStart;
	public GameObject rightLegsEnd;
	public GameObject leftLegsStart;
	public GameObject leftLegsEnd;

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

		if(rightLegsStart == null ||
		   rightLegsStart == null ||
		   rightLegsStart == null ||
		   rightLegsStart == null)
		{
			Debug.Log("ERROR: no reference for buff application points on this leg");
		}
	}

	//applies the buff to the selected leg
	public override TaskStatus OnUpdate()
	{
		if(_buffType == null)
		{
			//the animation to apply the buff! 
			if(_blackboard.selectedLeg.GetComponent<LegScript>()._id < 4)	//these are the right legs
			{

			}
			else if(_blackboard.selectedLeg.GetComponent<LegScript>()._id >= 4)	//these are the left legs
			{

			}

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
		}
		//no buff type set? return fail
		return TaskStatus.Failure;
	}

}
