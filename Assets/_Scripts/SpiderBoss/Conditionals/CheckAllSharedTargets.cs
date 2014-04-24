using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckAllSharedTargets : Action
{
	private bool _targetFound;

	public SharedGameObject _leg1Target;
	public SharedGameObject _leg2Target;
	public SharedGameObject _leg3Target;
	public SharedGameObject _leg4Target;
	public SharedGameObject _leg5Target;
	public SharedGameObject _leg6Target;
	public SharedGameObject _leg7Target;
	public SharedGameObject _leg8Target;
	private SharedGameObject[] _legs = new SharedGameObject[8];

	public override void OnAwake()
	{
		_legs[0] = _leg1Target;
		_legs[1] = _leg2Target;
		_legs[2] = _leg3Target;
		_legs[3] = _leg4Target;
		_legs[4] = _leg5Target;
		_legs[5] = _leg6Target;
		_legs[6] = _leg7Target;
		_legs[7] = _leg8Target;
	}

	public override void OnStart()
	{
		_targetFound = false;
	}

	public override TaskStatus OnUpdate()
	{
		foreach(SharedGameObject x in _legs)
		{
			if(x.Value != null)
			{
				_targetFound = true;
			}
		}

		if(_targetFound == true)
		{
			return TaskStatus.Failure;
		}
		else
		{
			return TaskStatus.Success;
		}
	}

}
