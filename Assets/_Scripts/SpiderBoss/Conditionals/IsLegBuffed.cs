using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class IsLegBuffed : Action 
{
	private BehaviorBlackboard _blackboard;
	private LegScript _legScript;

	public GameObject _leg;

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
		_legScript = _leg.GetComponent<LegScript>();
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_legScript != null)
		{
			//is the leg buffed with the matching type we want to find?
			if(_legScript._buffState == LegScript.BuffState.venom &&
			   _buffType == BuffType.venom)
			{
				return TaskStatus.Success;
			}
			else if(_legScript._buffState == LegScript.BuffState.web &&
			   _buffType == BuffType.web)
			{
				return TaskStatus.Success;
			}
			else
			{
				return TaskStatus.Failure;
			}
		}

		Debug.Log("There was no leg to check for buffs. This should never show up");
		return TaskStatus.Running;
	}
}
