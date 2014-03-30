using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class MoveToPoint : Action 
{

	public Transform target = null; 
	public float speed = 0; 
	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		// Return a task status of success once we've reached the target
		if (Vector3.SqrMagnitude(transform.position - target.position) < 0.1f) 
		{
			_blackboard.moveDirection = new Vector2(0, 0);
			return TaskStatus.Success;
		}
		// We haven't reached the target yet so keep moving towards it
		transform.position = Vector3.MoveTowards(transform.position, target.position, speed * (Time.deltaTime* StaticData.t_scale));

		Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
		direction.Normalize();
		_blackboard.moveDirection = direction;
		return TaskStatus.Running; 
	}

	public override void OnReset()
	{
		target = null; 
		speed = 0;
	}

}
