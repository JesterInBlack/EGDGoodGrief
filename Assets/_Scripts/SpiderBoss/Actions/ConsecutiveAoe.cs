using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ConsecutiveAoe : Action
{
	public SharedVector2 _inputVector;
	public SharedVector2 _offsetVector;

	public float _offsetMagnitude;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_offsetMagnitude = 3.0f;
	}

	public override void OnStart()
	{

	}

	public override TaskStatus OnUpdate()
	{
		Vector2 rand = Random.insideUnitCircle;
		rand.Normalize();
		_offsetVector.Value = _inputVector.Value + rand * _offsetMagnitude; 
		return TaskStatus.Success;
	}
}