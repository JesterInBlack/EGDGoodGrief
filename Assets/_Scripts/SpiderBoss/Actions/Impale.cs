using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class Impale : Action
{
	#region vars
	private BehaviorBlackboard _blackboard;
	private Vector2 _targetPoint;
	private Vector2 _intermediatePoint;
	private Vector2 _startPoint;

	public SharedGameObject _optionalSetTarget;
	private GameObject _targetPlayer;
	public int _setLeg = -1;

	private LegScript _selectedLeg;
	private GameObject _selectedPoint;

	private float _lerpTime;
	public float _windupTime;
	public float _aimTime;
	public float _attackTime;
	public float _cooldownTime;

	public float radius = 1.0f;    //TODO: make < 1
	private const float damage = 20.0f; //suck it, Unity! (I've had enough of your saving public vars Bullshit)
	#endregion

	public enum AttackState
	{
		Windup = 0,
		Aim = 1,
		Attack = 2,
		Cooldown = 3,
	}
	public AttackState _state;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_windupTime = 0.3f;
		_aimTime = 1.0f;
		_attackTime = 0.1f;
		_cooldownTime = 1.25f;
	}
	
	public override void OnStart()
	{
		_state = AttackState.Windup;
		if(_setLeg == -1)
		{
			_selectedLeg = _blackboard.selectedLeg;
			_selectedPoint = _blackboard.selectedPoint;
		}
		else
		{
			_selectedLeg = _blackboard.legsList[_setLeg];
			_selectedPoint = _blackboard.points[_setLeg];
		}
		_selectedLeg._state = LegScript.LegState.Idle;
		_selectedLeg._behaviorState = LegScript.BehaviorState.Impale;
		_selectedLeg._invincible = true;

		_startPoint = (Vector2)_selectedLeg.transform.position;
		_targetPoint = (Vector2)_selectedLeg.transform.position;
		_intermediatePoint = new Vector2(_selectedPoint.transform.position.x, _selectedPoint.transform.position.y + 5f);

		if(_optionalSetTarget.IsShared == true)
		{
			_targetPlayer = _optionalSetTarget.Value;
		}
		else
		{
			_targetPlayer = _blackboard.targetPlayer;
		}

		_lerpTime = 0.0f;

		//this is for the shadow
		_selectedLeg._lerpTime = 0.0f;
		_selectedLeg._startPoint = _startPoint;
		_selectedLeg._shadowIntermediatePoint = _targetPoint;
		_selectedLeg._shadowMoveTime = _windupTime;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_state == AttackState.Windup)
		{
			if( Vector2.Distance((Vector2)_selectedLeg.transform.position, _intermediatePoint) < 0.01f)
			{
				_state = AttackState.Aim;
				_lerpTime = 0.0f;
				_startPoint = (Vector2)_selectedLeg.transform.position;
				_intermediatePoint = Vector2.Lerp(_startPoint, (Vector2)_targetPlayer.transform.position, 0.15f);
			}
			else
			{
				_selectedLeg.transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _windupTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Aim)
		{
			TrackPlayer();

			if( Vector2.Distance((Vector2)_selectedLeg.transform.position, _intermediatePoint) < 0.001f)
			{
				_state = AttackState.Attack;
				_lerpTime = 0.0f;
				_startPoint = (Vector2)_selectedLeg.transform.position;
			}
			else
			{
				_selectedLeg.transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _aimTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Attack)
		{
			if( Vector2.Distance((Vector2)_selectedLeg.transform.position, _targetPoint) < 0.005f)
			{
				_selectedLeg._invincible = false;
				_state = AttackState.Cooldown;
				_lerpTime = 0.0f;

				//Do damage.
				AttackSystem.hitCircle ( _targetPoint, radius, damage, -1 );
				gameObject.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossImpale );
				GameState.cameraController.Shake ( 0.1f, 0.25f );

				//Debug.Log("does it ever reach here?");

				#region poison
				//If poisonous, go through all hit objects, find the players, and poison them.
				if ( _selectedLeg.GetComponent<LegScript>().isPoisonous() )
				{
					foreach ( Collider2D hit in AttackSystem.getHitsInCircle( _targetPoint, radius, -1) )
					{
						Player tempPlayer = hit.gameObject.GetComponent<Player>();
						if ( tempPlayer != null ) //this is a player.
						{
							LegScript tempLeg = _selectedLeg.GetComponent<LegScript>();
							tempPlayer.Poison ( tempLeg._poisonDuration, tempLeg._poisonDPS );
						}
					}
				}
				#endregion
			}
			else
			{
				//Debug.Log("winding up");
				_selectedLeg.transform.position = Vector2.Lerp(_startPoint, _targetPoint, _lerpTime / _attackTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else if(_state == AttackState.Cooldown)
		{
			if(_lerpTime > _cooldownTime)
			{

				//Debug.Log("It still finishes fine");
				return TaskStatus.Success;
			}
			else
			{
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}

		return TaskStatus.Running;
	}

	public void TrackPlayer()
	{
		_targetPoint = Vector2.MoveTowards(_targetPoint, (Vector2)_targetPlayer.transform.position, 2.5f * (Time.deltaTime* StaticData.t_scale));
		_selectedLeg._shadowIntermediatePoint = _targetPoint;
	}

	public override void OnEnd()
	{
		_selectedLeg._behaviorState = LegScript.BehaviorState.Walking;
		if(_optionalSetTarget.IsShared == true)
		{
			_optionalSetTarget.Value = null;
		}
	}
}
