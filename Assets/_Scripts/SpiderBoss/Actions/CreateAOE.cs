using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class CreateAOE : Action 
{
	public GameObject _venomPool;
	public GameObject _webField;
	
	private Vector2 _targetPosition;
	private float _offsetRadius = 2.0f;
	
	public enum AoeType
	{
		web = 0,
		venom = 1,
	}
	public AoeType _aoeType;
	
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		
		if(_venomPool == null ||
		   _webField == null)
		{
			Debug.Log("ERROR: no reference for Venom/Web field");
		}
	}

	public override void OnStart()
	{
		//set the target position
		Vector2 randomPoint = Random.insideUnitCircle;
		_targetPosition = (Vector2)_blackboard.targetPlayer.transform.position;
		_targetPosition.x += randomPoint.x * _offsetRadius;
		_targetPosition.y += randomPoint.y * _offsetRadius;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_aoeType == AoeType.venom)
		{
			Instantiate(_venomPool, _targetPosition, Quaternion.identity);
			return TaskStatus.Success;
		}
		else if(_aoeType == AoeType.web)
		{
			Instantiate(_webField, _targetPosition, Quaternion.identity);
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;
		
	}
}
