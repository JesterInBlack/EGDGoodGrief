using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireTetherProjectile : Action
{
	public SharedGameObject _spawnedTether;

	public GameObject _webTetherProjectile;
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

		GameObject shot = Instantiate(_webTetherProjectile, _startingPosition.transform.position, Quaternion.identity) as GameObject;
		shot.GetComponent<TetherProjectileScript>().Initializer((Vector2)_startingPosition.transform.position, _targetPlayer);
		_spawnedTether.Value = shot;

		return TaskStatus.Success;
	}
}
