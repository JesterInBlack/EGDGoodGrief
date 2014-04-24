using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class DissentionSuction : Action
{
	public GameObject _suctionParticles;
	public GameObject _spawnedSuction;

	public float _chargeTime;
	public float _chargeDuration;
	public float _riseDuration;
	public float _recoveryDuration;
	public float _bumpHeight;

	public float _lerpTime;
	private Vector2 _startingPos;
	private Vector2 _chargePos;
	private Vector2 _groundedPos;

	private Vector2 shake;
	private float shakeMagnitude;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_bumpHeight = 1.5f;
		_riseDuration = 2.0f;
		_recoveryDuration = 4.0f;
		_chargeDuration = 15.0f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Suction;
		_blackboard.body._bodyState = BodyScript.BodyState.Rising;

		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _bumpHeight);

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);

		_lerpTime = 0.0f;
		_chargeTime = 0.0f;

		shake = new Vector3( 0.0f, 0.0f, 0.0f );
		shakeMagnitude = 0.05f;
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
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
				_lerpTime = 0.0f;
				_spawnedSuction = Instantiate(_suctionParticles, _groundedPos, Quaternion.identity) as GameObject;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _riseDuration);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Attacking)
		{
			if(_chargeTime <= _chargeDuration)
			{
				_chargeTime += Mathf.Min(Time.deltaTime * StaticData.t_scale, _chargeDuration - _chargeTime);
			}
			ShakeBoss(_chargeTime / _chargeDuration);

			//TODO make the suction thing
			AttackSystem.Suck(_blackboard.body._shadowPos, _chargeTime, Time.deltaTime * StaticData.t_scale);
			if ( ! gameObject.GetComponent<AudioSource>().isPlaying )
			{
				gameObject.GetComponent<AudioSource>().loop = true;
				gameObject.GetComponent<AudioSource>().clip = SoundStorage.BossSuction;
				gameObject.GetComponent<AudioSource>().Play ();
			}
		}

		return TaskStatus.Running;
	}

	//make the boss shake harder over time
	void ShakeBoss(float time)
	{
		float x = Random.Range ( -shakeMagnitude * time, shakeMagnitude * time);
		float y = Random.Range ( -shakeMagnitude * time, shakeMagnitude * time);
		shake = new Vector2( x, y );
		transform.position = _chargePos + shake;
	}

	public override void OnEnd()
	{
		shake = new Vector2(0, 0);
		transform.position = _chargePos;
		gameObject.GetComponent<AudioSource>().Stop ();
		_spawnedSuction.GetComponent<SuctionParticleScript>().Kill();
	}
}
