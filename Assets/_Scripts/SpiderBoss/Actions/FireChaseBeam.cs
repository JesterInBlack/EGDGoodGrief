using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireChaseBeam : Action
{
	public GameObject _laserSpawnPoint;
	public GameObject _laserObject;
	private GameObject _spawnedLaser;
	public GameObject _chargeObject;
	private GameObject _spawnedCharge;

	private float _lastSpawnedTime;
	private float _spawnDuration = 0.1f;

	public float _chaseSpeed = 6.0f;

	public float _chargeDuration;
	public float _laserDuration;
	public float _timer;

	private Player _targetPlayer;
	private EyeScript _eyesScript;
	private BehaviorBlackboard _blackboard;
	private bool _noTargetsLeft;

	private enum State
	{
		Charging = 0,
		Firing = 1,
		Done = 2,
	}
	private State _state;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_eyesScript = _blackboard.eye.GetComponent<EyeScript>();
		_laserSpawnPoint = _eyesScript._laserSpawnPoint;
		_chargeDuration = _chargeObject.GetComponent<Lifetime>().lifetime;
		_laserDuration = _laserObject.GetComponent<Lifetime>().lifetime;
	}

	public override void OnStart()
	{
		_targetPlayer = _blackboard.targetPlayer.GetComponent<Player>();
		_eyesScript._behaviorState = EyeScript.BehaviorStates.ChaseBeam;
		_timer = 0.0f;
		//_lastSpawnedTime = _chaseTime + _spawnDuration;
		_noTargetsLeft = false;
		GameState.cameraController.Shake (0.01f, _chargeDuration + _laserDuration);
		_state = State.Charging;

		_spawnedCharge = Instantiate(_chargeObject) as GameObject;
		_spawnedCharge.transform.position = _laserSpawnPoint.transform.position;
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		//return TaskStatus.Running;
		if(_noTargetsLeft == false)
		{
			if(_state == State.Charging)
			{
				if(_timer < _chargeDuration)
				{
					_timer += Time.deltaTime;
					_spawnedCharge.transform.position = _laserSpawnPoint.transform.position;
					_spawnedCharge.transform.eulerAngles = _eyesScript._rotationVec;
				}
				else
				{
					_state = State.Firing;
					_timer = 0.0f;

					_spawnedLaser = Instantiate(_laserObject) as GameObject;
					_spawnedLaser.transform.position = _laserSpawnPoint.transform.position;
					_spawnedLaser.transform.eulerAngles = _eyesScript._rotationVec;
				}
			}
			else if(_state == State.Firing)
			{
				if(_timer < _laserDuration)
				{
					_timer += Time.deltaTime;
					_spawnedLaser.transform.position = _laserSpawnPoint.transform.position;
					_spawnedLaser.transform.eulerAngles = _eyesScript._rotationVec;
				}
				else
				{
					_timer = 0.0f;
					return TaskStatus.Success;
				}
			}


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
		}
		else
		{
			GameState.cameraController.Shake (0.0f, 0.0f);
			return TaskStatus.Failure;
		}
		return TaskStatus.Running;
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
