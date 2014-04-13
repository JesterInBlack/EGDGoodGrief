using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class CreateAOE : Action 
{
	public GameObject _startingPosition;
	public GameObject _venomShot;
	public GameObject _webShot;
	
	private Vector2 _targetPosition;
	private Vector2 _shadowStartPos;
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
		_startingPosition = GameObject.Find("ProjectileStartPoint");

		if(_venomShot == null ||
		   _webShot == null)
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

		_shadowStartPos = (Vector2)_startingPosition.transform.position;
		_shadowStartPos.y -= 2.5f;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{

		if(_aoeType == AoeType.venom)
		{
			GameObject shot = Instantiate(_venomShot, _startingPosition.transform.position, Quaternion.identity) as GameObject;
			shot.GetComponent<LobProjectile>().Initializer((Vector2)_startingPosition.transform.position, _shadowStartPos, _targetPosition);
			return TaskStatus.Success;
		}
		else if(_aoeType == AoeType.web)
		{
			GameObject shot = Instantiate(_webShot, _startingPosition.transform.position, Quaternion.identity) as GameObject;
			shot.GetComponent<LobProjectile>().Initializer((Vector2)_startingPosition.transform.position, _shadowStartPos, _targetPosition);
			return TaskStatus.Success;
		}
		return TaskStatus.Failure;
		
	}
}
