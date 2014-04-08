using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class UnityMegaFlareSharedLegs : Action 
{
	private BehaviorBlackboard _blackboard;

	public int[] _selectedLegs = new int[4];
	public SharedInt _leg1;
	public SharedInt _leg2;
	public SharedInt _leg3;
	public SharedInt _leg4;

	public override void OnAwake()
	{
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override void OnStart()
	{
		_selectedLegs = _blackboard.selectedLegs;
	}

	public override TaskStatus OnUpdate()	
	{
		
		_leg1.Value = _selectedLegs[0];
		_leg2.Value = _selectedLegs[1];
		_leg3.Value = _selectedLegs[2];
		_leg4.Value = _selectedLegs[3];
		
		return TaskStatus.Success;
		
	}
}
