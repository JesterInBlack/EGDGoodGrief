using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("BlackboardSet")]
public class SetEyeRotationSpeed : Action
{
	public float _rotationSpeed;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		if(_rotationSpeed == 0)
		{
			_rotationSpeed = _blackboard.eye._rotationSpeed = _blackboard.eye._defaultRotationSpeed;
		}
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		_blackboard.eye._rotationSpeed = _rotationSpeed;
		return TaskStatus.Success;
	}
}
