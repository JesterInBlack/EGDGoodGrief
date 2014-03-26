using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class MoveToPoint : Action 
{

	//public Transform target = null;
	public Vector3 target = new Vector3( 10.0f, 0.0f, 0.0f );
	public float speed = 0; 

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		// Return a task status of success once we've reached the target
		if (Vector3.SqrMagnitude(transform.position - target/*.position*/) < 0.1f) 
		{
			return TaskStatus.Success;
		}
		// We haven't reached the target yet so keep moving towards it
		transform.position = Vector3.MoveTowards(transform.position, target/*.position*/, speed * Time.deltaTime);
		return TaskStatus.Running; 
	}

	public override void OnReset()
	{
		target = new Vector3(0.0f, 0.0f, 0.0f); //null; 
		speed = 0;
	}

}
