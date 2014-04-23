using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class BodySlam : Action
{
	public SharedGameObject _optionalObjectToDestroy;
	public GameObject _pointOfDamage;
	public GameObject _shockwaveSpawner;

	public float _bumpHeight;
	public float _chargeTime;
	public float _fallTime;
	public float _riseTime;
	public float _slamDuration;
	public float _slamTimer;

	private float _lerpTime;
	private Vector2 _chargePos;
	private Vector2 _startingPos;
	private Vector2 _groundedPos;
	private Vector2 _endingPos;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_pointOfDamage = GameObject.Find("ProjectileStartPoint");

		_chargeTime = 0.85f;
		_fallTime = 0.125f;
		_riseTime = 0.85f;
		_slamDuration = 1.0f;
	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.BodySlam;
		_blackboard.body._bodyState = BodyScript.BodyState.Charging;

		_slamTimer = 0.0f;
		_lerpTime = 0.0f;

		_bumpHeight = 1.5f;
		_startingPos = (Vector2)transform.position;
		_chargePos = new Vector2(_startingPos.x, _startingPos.y + _bumpHeight);
		_endingPos = (Vector2)_blackboard.body._neutralPoint.transform.position;

		float groundedHeight = Vector2.Distance(_startingPos, _blackboard.body._shadowPos);
		groundedHeight *= _blackboard.body._groundHeightOffset;
		_groundedPos = new Vector2(_blackboard.body._shadowPos.x, _blackboard.body._shadowPos.y + groundedHeight);

		_blackboard.eye.GetComponent<EyeScript>()._behaviorState = EyeScript.BehaviorStates.LookDown;
	}
	
	public override TaskStatus OnUpdate ()
	{
		float currentHeight = Vector2.Distance((Vector2)transform.position, _groundedPos);
		float maxHeight = Vector2.Distance(_startingPos, _groundedPos);
		_blackboard.body._height = (currentHeight/maxHeight) * _blackboard.body._baseHeight;

		if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(Vector2.Distance( (Vector2)transform.position, _chargePos) < 0.001f)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Falling;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startingPos, _chargePos, _lerpTime / _chargeTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Falling)
		{
			if(Vector2.Distance( (Vector2)transform.position, _groundedPos) < 0.001f)
			{
				//destory the optional object if there is one
				if(_optionalObjectToDestroy != null)
				{
					Destroy(_optionalObjectToDestroy.Value);
				}
				this.gameObject.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossImpale, 1.0f );

				Instantiate(_shockwaveSpawner, _pointOfDamage.transform.position, Quaternion.identity);
				AttackSystem.hitCircle((Vector2)_pointOfDamage.transform.position, 3.5f, 30.0f, -1);
				GameState.cameraController.Shake (0.125f, 0.3f );

				_blackboard.body._bodyState = BodyScript.BodyState.OnGound;
				_lerpTime = 0.0f;
				_blackboard._invincible = false;
			}
			else
			{
				transform.position = Vector2.Lerp(_chargePos, _groundedPos, _lerpTime / _fallTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.OnGound)
		{
			if(_slamTimer >= _slamDuration)
			{
				_blackboard.body._bodyState = BodyScript.BodyState.Rising;
				_blackboard._invincible = true;
			}
			else
			{
				_slamTimer += Time.deltaTime * StaticData.t_scale;
			}
		}
		else if (_blackboard.body._bodyState == BodyScript.BodyState.Rising)
		{
			if(Vector2.Distance( (Vector2)transform.position, _endingPos) < 0.001f)
			{
				return TaskStatus.Success;
			}
			else
			{
				transform.position = Vector2.Lerp(_groundedPos, _endingPos, _lerpTime / _riseTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		return TaskStatus.Running;
	}

	public override void OnEnd()
	{
		_blackboard.body._bodyState = BodyScript.BodyState.Floating;
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Healthy;
		_blackboard.eye.GetComponent<EyeScript>()._behaviorState = EyeScript.BehaviorStates.Idle;
	}
}
