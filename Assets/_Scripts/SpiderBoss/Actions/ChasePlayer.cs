using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class ChasePlayer : Action 
{
	public float _speed;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_speed = 1.0f;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		/*
		// Return a task status of success once we've reached the target
		if (Vector2.Distance((Vector2)transform.position, (Vector2)_blackboard.targetPlayer.transform.position) < 0.1f) 
		{
			_blackboard.moveDirection = new Vector2(0, 0);
			return TaskStatus.Success;
		}
		*/

		// We haven't reached the target yet so keep moving towards it
		transform.position = Vector2.MoveTowards((Vector2)transform.position, (Vector2)_blackboard.targetPlayer.transform.position, _speed * Time.deltaTime);

		Vector2 direction = (Vector2)_blackboard.targetPlayer.transform.position - (Vector2)transform.position;
		direction.Normalize();
		_blackboard.moveDirection = direction;

		return TaskStatus.Running; 
	}
}
