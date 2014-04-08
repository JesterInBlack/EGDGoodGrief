using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class SetTargetLegShared : Action
{
	public SharedInt _sharedLeg;
	public int _legNumber;
	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override void OnStart()
	{
		_legNumber = _sharedLeg.Value;

	}
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		_blackboard.selectedLeg = _blackboard.legsList[_legNumber];
		return TaskStatus.Success;
	}
}
