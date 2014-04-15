using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireChaseBeam : Action
{
	public GameObject _laserSpawnPoint;
	public GameObject _eyeLaser;
	private float _lastSpawnedTime;
	private float _spawnDuration = 0.1f;

	public float _chaseSpeed = 50.0f;

	public float _chaseDuration = 10.0f;
	public float _chaseTime;

	private Player _targetPlayer;
	private EyeScript _eyesScript;
	private BehaviorBlackboard _blackboard;
	private bool _noTargetsLeft;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_eyesScript = _blackboard.eye.GetComponent<EyeScript>();
		_laserSpawnPoint = _eyesScript._laserSpawnPoint;
	}

	public override void OnStart()
	{
		_targetPlayer = _blackboard.targetPlayer.GetComponent<Player>();
		_eyesScript._behaviorState = EyeScript.BehaviorStates.ChaseBeam;
		_chaseTime = 0.0f;
		_lastSpawnedTime = _chaseTime + _spawnDuration;
		_noTargetsLeft = false;
		GameState.cameraController.Shake (0.01f, _chaseDuration);
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_noTargetsLeft == false)
		{
			if(_chaseTime >= _chaseDuration)
			{
				return TaskStatus.Success;
			}
			else
			{
				//Spawn the laser parts and send em on their way
				if(_chaseTime - _lastSpawnedTime >= _spawnDuration)
				{

					Vector3 spawnPos = _laserSpawnPoint.transform.position;
					spawnPos.z = _chaseTime;
					GameObject lzr =  Instantiate(_eyeLaser) as GameObject;
					lzr.GetComponent<EyeLaserScript>().Initializer(spawnPos, _eyesScript._rotationVec.z);

					_lastSpawnedTime = _chaseTime;
				}

				//increment the time
				_chaseTime += Time.deltaTime * StaticData.t_scale;

				if(_targetPlayer.isDowned == false)
				{
					_eyesScript.GetTargetAngle(_targetPlayer.transform.position);
					_eyesScript.RotateToTarget(_chaseSpeed);
				}
				//find a the next target and keep going!
				else
				{
					//Debug.Log("Switching target!");
					GetNewTarget();
				}
				return TaskStatus.Running;
			}
		}
		else
		{
			GameState.cameraController.Shake (0.0f, 0.0f);
			return TaskStatus.Failure;
		}
	}

	void GetNewTarget()
	{
		float highestAggro = 0;
		int aggroIndex = -1;
		for(int i = 0; i < GameState.players.Length; i++)
		{
			if(GameState.players[i].GetComponent<Player>().isDowned == false && GameState.playerThreats[i] >= highestAggro)
			{
				highestAggro = GameState.playerThreats[i];
				aggroIndex = i;
			}
		}
		
		if(aggroIndex == -1)
		{
			_noTargetsLeft = true;
		}
		else
		{
			//Debug.Log("new target is player: " + aggroIndex);
			_targetPlayer = GameState.players[aggroIndex].GetComponent<Player>();
		}
	}

	public override void OnEnd()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.Idle;
	}
}
