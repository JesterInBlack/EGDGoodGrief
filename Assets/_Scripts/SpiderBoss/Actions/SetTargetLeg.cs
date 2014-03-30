using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class SetTargetLeg : Action 
{
	public int _LegNumber;							//between 1 and 8, it goes right1234 -> left1234
	private BehaviorBlackboard _blackboard;

	public enum TargetType
	{
		selection = 0,
		random = 1,
	}
	public TargetType _targetType;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_targetType == TargetType.selection)
		{
			_blackboard.selectedLeg = _blackboard.legsList[_LegNumber];
			return TaskStatus.Success;
		}
		else if(_targetType == TargetType.random)
		{
			int randomIndex = Random.Range(0, 7);
			_blackboard.selectedLeg = _blackboard.legsList[randomIndex];
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;
		
	}
}
