using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireExplodingEggProjectile : Action
{
	public GameObject _explodingSacProjectile;
	public GameObject _startingPosition;
	private GameObject _targetPlayer;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_startingPosition = GameObject.Find("Abdomen/WebShotPoint");
		
	}
	
	public override void OnStart()
	{
		//set the target position
		_targetPlayer = _blackboard.targetPlayer;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		
		GameObject shot = Instantiate(_explodingSacProjectile, _startingPosition.transform.position, Quaternion.identity) as GameObject;
		shot.GetComponent<ExplodingSacProjectileScript>().Initializer((Vector2)_startingPosition.transform.position, _targetPlayer);
		
		return TaskStatus.Success;
	}
}
