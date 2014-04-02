using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;

public class FindUnbuffedLeg : Action 
{
	private BehaviorBlackboard _blackboard;
	private List<int> _unbuffedLegs;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_unbuffedLegs = new List<int>();
	}

	//randomly picks an unbuffed leg
	public override TaskStatus OnUpdate()
	{
		for(int i = 0; i < _blackboard.legsList.Length; i++)
		{
			if(_blackboard.legsList[i]._buffState == LegScript.BuffState.unbuffed)
			{
				_unbuffedLegs.Add(i);
			}
		}

		//return fail if there's no unbuffed leg
		if(_unbuffedLegs.Count == 0)
		{
			return TaskStatus.Failure;
		}

		//otherwise randomly set one of the unbuffed legs and return success
		int selectedInt = Random.Range(0, _unbuffedLegs.Count);
		int selectedLeg = _unbuffedLegs[selectedInt];
		_blackboard.selectedLeg = _blackboard.legsList[selectedLeg];
		return TaskStatus.Success;
	}
}
