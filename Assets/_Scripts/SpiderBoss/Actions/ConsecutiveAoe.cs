using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ConsecutiveAoe : Action
{
	public GameObject _aoeObject;

	public SharedVector2 _offsetVector;

	private float _offsetMagnitude;
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
	}

	public override void OnStart()
	{
		_offsetMagnitude = _aoeObject.GetComponent<CircleCollider2D>().radius * _aoeObject.transform.lossyScale.x;
	}

	public override TaskStatus OnUpdate()
	{
		Vector2 rand = Random.insideUnitCircle;
		rand.Normalize();
		_offsetVector = _offsetVector + rand * _offsetMagnitude; 
		return TaskStatus.Success;
	}
}