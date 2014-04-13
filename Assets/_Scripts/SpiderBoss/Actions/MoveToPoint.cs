using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class MoveToPoint : Action 
{
	public SharedGameObject _targetObject;

	public GameObject _target = null; 

	private Vector2 _offsetPos;
	public float _speed = 0; 
	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override void OnStart ()
	{
		_target = _targetObject.Value; 

		_offsetPos = (Vector2)_target.transform.position;
		_offsetPos.y += _blackboard.body._height;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		// Return a task status of success once we've reached the target
		if (Vector2.SqrMagnitude((Vector2)transform.position - _offsetPos) < 0.001f) 
		{
			_blackboard.moveDirection = new Vector2(0, 0);
			return TaskStatus.Success;
		}
		// We haven't reached the target yet so keep moving towards it
		transform.position = Vector2.MoveTowards((Vector2)transform.position, _offsetPos, _speed * (Time.deltaTime* StaticData.t_scale));

		Vector2 direction = _offsetPos - (Vector2)transform.position;
		direction.Normalize();
		_blackboard.moveDirection = direction;
		return TaskStatus.Running; 
	}

	public override void OnReset()
	{
		_target = null; 
		_speed = 0;
	}

}
