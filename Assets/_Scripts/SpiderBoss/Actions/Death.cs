using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks;

public class Death : Action
{
	public float _chargeTime;
	public float _chargeDuration;
	public float _recoveryDuration;
	private float _explosionTimer;
	private float _explosionTime;

	private Vector2 _startingPos;

	private Vector2 _shake;
	private float _shakeMagnitude;

	public GameObject[] _legParts;
	public GameObject[] _legShadows;

	private bool _addWeight;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_explosionTime = 0.0f;
		_explosionTimer = 0.15f;
		_shakeMagnitude = 0.15f;
		_chargeDuration = 2.5f;
		_recoveryDuration = 4.0f;

		//_legParts = new List<GameObject>();
		_legParts = GameObject.FindGameObjectsWithTag("LegSegment");
		_legShadows = GameObject.FindGameObjectsWithTag("LegShadow");

		_addWeight = false;

	}

	public override void OnStart()
	{
		_blackboard.body._behaviorState = BodyScript.BehaviorState.Dying;
		_blackboard.body._bodyState = BodyScript.BodyState.Charging;
		
		_chargeTime = 0.0f;
		
		_startingPos = (Vector2)transform.position;
		
		_shake = new Vector3( 0.0f, 0.0f, 0.0f );

		//play dead sound
		this.gameObject.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossDeathScream, 1.0f );

		foreach(LegScript x in _blackboard.legsList)
		{
			x._behaviorState = LegScript.BehaviorState.Dead;
		}
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_blackboard.body._bodyState == BodyScript.BodyState.Charging)
		{
			if(_chargeTime >= _chargeDuration)
			{
				transform.position = _startingPos;
				_blackboard.body._bodyState = BodyScript.BodyState.Attacking;
				_chargeTime = 0.0f;
			}
			else
			{
				if(_chargeTime > _explosionTime)
				{
					_explosionTime += _explosionTimer;
					this.gameObject.GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossImpale, 0.75f );
				}
				ShakeBoss(_chargeTime / _chargeDuration);
				_chargeTime += Time.deltaTime * StaticData.t_scale;
			}
		}

		else if(_blackboard.body._bodyState == BodyScript.BodyState.Attacking)
		{
			if(_addWeight == false)
			{
				foreach(GameObject x in _legShadows)
				{
					Destroy(x);
				}

				foreach(GameObject x in _legParts)
				{
					x.GetComponent<HingeJoint2D>().enabled = false;
					x.GetComponent<Rigidbody2D>().fixedAngle = false;
					x.GetComponent<Rigidbody2D>().gravityScale = 3.0f;
					if(x.transform.localPosition.x < 0)
					{
						//x.GetComponent<Rigidbody2D>().AddForce(Vector2.right * -0.02f + Vector2.up * 0.09f);
						x.GetComponent<Rigidbody2D>().AddForce(Vector2.right * Random.Range(-0.01f, -0.03f) + Vector2.up * Random.Range(0.03f, 0.09f));
						x.GetComponent<Rigidbody2D>().AddTorque(Random.Range(0.05f, 0.12f));
					}
					else if(x.transform.localPosition.x > 0)
					{
						x.GetComponent<Rigidbody2D>().AddForce(Vector2.right * Random.Range(0.01f, 0.03f) + Vector2.up * Random.Range(0.03f, 0.09f));
						x.GetComponent<Rigidbody2D>().AddTorque(Random.Range(-0.01f, -0.12f));
					}
				}
				_addWeight = true;
			}
			else
			{
				foreach(GameObject x in _legParts)
				{

					x.GetComponent<Rigidbody2D>().mass = 1.0f;
				}
				_blackboard.body._bodyState = BodyScript.BodyState.Recovery;
			}
		}
		else if(_blackboard.body._bodyState == BodyScript.BodyState.Recovery)
		{
			if(_chargeTime >= _recoveryDuration)
			{
				_chargeTime = 0.0f;
				return TaskStatus.Success;
			}
			else
			{
				_chargeTime += Time.deltaTime * StaticData.t_scale;
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
		transform.position = _startingPos + _shake;
	}
}
