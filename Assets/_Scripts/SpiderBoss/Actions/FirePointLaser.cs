using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FirePointLaser : Action
{
	public GameObject _pointLaser;
	private GameObject _spawnedPointLaser;

	public Vector2 _targetPoint;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

	}

	public override void OnStart()
	{
		//get the target point of the laser
		_targetPoint = (Vector2)_blackboard.targetPlayer.transform.position;
	}

	public override TaskStatus OnUpdate()
	{
		gameObject.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossPointLaser, 1.0f );
		_spawnedPointLaser = Instantiate(_pointLaser, _blackboard.eye._laserSpawnPoint.transform.position, Quaternion.identity) as GameObject;
		_spawnedPointLaser.GetComponent<PointLaserScript>().Initializer((Vector2)_blackboard.eye._laserSpawnPoint.transform.position, _targetPoint);
		return TaskStatus.Success;
	}

}
