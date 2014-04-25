using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class UnityMegaFlare : Action
{
	public GameObject _megaFlareLaser;
	private EyeScript _eyeScript;
	private GameObject _spawnedMegaFlare;

	public SharedBool _sharedFinishedAttack;

	private BehaviorBlackboard _blackboard;

	public float _chargeTime;
	public float _chargeDuration;
	public float _riseDuration;
	public float _attackDuration;
	public float _recoveryDuration;
	public float _bumpHeight;

	private float _lerpTime;
	private Vector2 _startingPos;
	private Vector2 _chargePos;
	private Vector2 _attackPos;
	private Vector2 _groundedPos;

	private Vector2 _shake;
	private float _shakeMagnitude;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_eyeScript = _blackboard.eye.GetComponent<EyeScript>();

		_bumpHeight = 1.5f;
		_riseDuration = 2.0f;
		_attackDuration = 0.1f;
		_recoveryDuration = 4.0f;
		_shakeMagnitude = 0.1f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.MegaFlare;
		_blackboard.body._bodyState = BodyScript.BodyState.Rising;

		_sharedFinishedAttack.Value = false;

		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _bumpHeight);
		_attackPos = new Vector2(_chargePos.x, _chargePos.y + (_bumpHeight/2));

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);

		_shake = new Vector3( 0.0f, 0.0f, 0.0f );

		//sets the charge time. The more legs are alive, the longer the charge
		int legsAlive = 0;
		for(int i = 0; i < _blackboard.legsList.Length; i++)
		{
			if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
			{
				legsAlive++;
			}
		}
		if(legsAlive <= 5)
		{
			_chargeDuration = 9.0f;
		}
		else if(legsAlive == 6)
		{
			_chargeDuration = 12.0f;
		}
		else if(legsAlive == 7)
		{
			_chargeDuration = 15.0f;
		}
		else if(legsAlive == 8)
		{
			_chargeDuration = 18.0f;
		}
		_chargeTime = 0.0f;
		_lerpTime = 0.0f;
	}

	public override TaskStatus OnUpdate()
	{

		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;
		
		if(_blackboard.body._bodyState == BodyScript.BodyState.Rising)
		{
			if(Vector2.Distance( (Vector2)transform.position, _chargePos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Charging;
				_lerpTime = 0.0f;

				//create the megaflare!
				_spawnedMegaFlare = Instantiate(_megaFlareLaser, _eyeScript._laserSpawnPoint.transform.position, Quaternion.identity) as GameObject;
				_spawnedMegaFlare.GetComponent<MegaFlareScript>().Initializer(_eyeScript._laserSpawnPoint, _eyeScript._laserSpawnPoint.transform.position, _blackboard.body._shadowPos, _chargeDuration);
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _riseDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(_chargeTime >= _chargeDuration)
			{
				_spawnedMegaFlare.GetComponent<MegaFlareScript>().Attack();
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
			}
			else
			{
				ShakeBoss(_chargeTime / _chargeDuration);
				_chargeTime += Time.deltaTime * StaticData.t_scale;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Attacking)
		{
			if(Vector2.Distance( (Vector2)transform.position, _attackPos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Recovery;
				_lerpTime = 0.0f;
				_sharedFinishedAttack.Value = true;
			}
			else
			{
				transform.position = Vector2.Lerp(_chargePos, _attackPos, _lerpTime / _attackDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Recovery)
		{
			if(Vector2.Distance( (Vector2)transform.position, _startingPos) < 0.001f)
			{
				return TaskStatus.Success;
			}
			else
			{
				transform.position = Vector2.Lerp(_attackPos, _startingPos, _lerpTime / _recoveryDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		return TaskStatus.Running;
	}

	//make the boss shake harder over time
	void ShakeBoss(float time)
	{
		float x = Random.Range ( -_shakeMagnitude * time, _shakeMagnitude * time);
		float y = Random.Range ( -_shakeMagnitude * time, _shakeMagnitude * time);
		_shake = new Vector2( x, y );
		transform.position = _chargePos + _shake;
	}

	public override void OnEnd()
	{
		if(_sharedFinishedAttack.Value == false)
		{
			_spawnedMegaFlare.GetComponent<MegaFlareScript>().Cancel();
		}
		else
		{
			_sharedFinishedAttack.Value = false;
		}
		_shake = new Vector2(0, 0);
		_blackboard.body._bodyState = BodyScript.BodyState.Floating;
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Healthy;
	}
}
