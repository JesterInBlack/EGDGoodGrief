using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class AliveCountLeg : Action
{
	private BehaviorBlackboard _blackboard;
	
	public int _legCountThreshold;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}
	
	public override TaskStatus OnUpdate()
	{
		int aliveCount = 0;
		for(int i = 0; i < _blackboard.legsList.Length; i++)
		{
			if(_blackboard.legsList[i].GetComponent<LegScript>()._currentHP > 0)
			{
				aliveCount++;
			}
		}
		
		if(aliveCount >= _legCountThreshold)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
