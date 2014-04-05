using UnityEngine;
using System.Collections;

public class LegScript : MonoBehaviour {

	#region vars
	public int _id;

	public float _radius = 2.5f;
	public float _movementFlux = 0.75f;
	public float _moveTime;
	public GameObject _disabledPoint;
	public GameObject _radiusPoint;
	public GameObject _bodyScript;

	private bool _recovering;
	public float _currentHP;
	public float _maxHP;
	public float _regenRate;
	public bool invincible; //true when the leg is immune to damage.

	//color vars
	ScheduledColor currentColor;

	#region LEG BUFF VARIABLES
	public float _currentWebbingHP;
	public float _maxWebbingHP;

	public float _currentBuffDuration;
	public float _maxBuffDuration;

	public float _poisonDPS;      //in damage per second
	public float _poisonDuration; //in seconds
	#endregion

	[HideInInspector]
	public Vector2 _shadowPos;
	[HideInInspector]
	public Vector2 _shadowIntermediatePoint;
	[HideInInspector]
	public float _shadowMoveTime;
	private Vector2 _shadowTargetPoint;
	private Vector2 _shadowStartPoint;

	private bool _intermediatePointReached;
	private Vector3 _intermediatePoint;
	private Vector3 _targetPoint;
	[HideInInspector]
	public Vector2 _startPoint;
	[HideInInspector]
	public float _lerpTime;
	#endregion

	public BehaviorBlackboard _blackboard;

	//enums
	public enum BehaviorState
	{
		Disabled = -1,
		Walking = 0,
		Impale = 1,
		Rake = 2,
		ApplyingBuff = 3,
	}
	public BehaviorState _behaviorState;

	public enum LegState
	{
		Idle = 0,
		CalculateMove = 1,
		Move = 2,
	}
	[HideInInspector]
	public LegState _state;

	public enum BuffState
	{
		unbuffed = 0,
		venom= 1,
		web = 2,
	}
	[HideInInspector]
	public BuffState _buffState;

	// Use this for initialization
	void Start () 
	{
		_buffState = BuffState.unbuffed;
		_behaviorState = BehaviorState.Walking;
		_state = LegState.Idle;

		_regenRate = 10.0f;
		_moveTime = 0.25f;
		_maxHP = 100.0f;
		_currentHP = _maxHP;

		_maxWebbingHP = 75.0f;
		_currentWebbingHP = 0.0f;

		_maxBuffDuration = 30.0f;
		_currentBuffDuration = 0.0f;

		currentColor = new ScheduledColor( new Color( 1.0f, 1.0f, 1.0f), 0.0f );
		_recovering = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_behaviorState == BehaviorState.Disabled)
		{
			if(_state == LegState.Idle)
			{
				transform.position = _disabledPoint.transform.position;

				_shadowPos = (Vector2)transform.position;
				if(_blackboard.body._behaviorState != BodyScript.BehaviorState.Disabled)
				{
					//_shadowPos.y -= 3.0f;
					_shadowPos.y -= _blackboard.body._height;
				}
			}
			else if(_state == LegState.CalculateMove)
			{
				_targetPoint = _disabledPoint.transform.position;
				_shadowTargetPoint = _targetPoint;
				_shadowTargetPoint.y -= _blackboard.body._height;

				_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, -1);
				_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
				_intermediatePointReached = false;
				_startPoint = transform.position;
				_lerpTime = 0.0f;
				_state = LegState.Move;

			}
			else if(_state == LegState.Move)
			{
				MoveLeg();
			}
		}
		else if(_behaviorState == BehaviorState.Walking)
		{
			if(_state == LegState.Idle)
			{
				//check to see if the leg is too far away from the radius point 
				if(Vector2.Distance(this.transform.position, _radiusPoint.transform.position) > _radius)
				{
					_state = LegState.CalculateMove;
				}
				_shadowPos = (Vector2)transform.position;
			}
			//This picks the next area to move to
			else if(_state == LegState.CalculateMove)
			{
				_targetPoint = _radiusPoint.transform.position + GetMoveVector();
				_shadowTargetPoint = _targetPoint;
				if(_recovering)
				{
					_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, -1);
					_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
					_recovering = false;
				}
				else
				{
					//TODO: FIX NULL REF ERROR
					_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint, 1);
					_shadowIntermediatePoint = GetIntermediatePoint(transform.position, _shadowTargetPoint, 0);
				}
				_intermediatePointReached = false;
				_startPoint = transform.position;
				_shadowStartPoint = _startPoint;
				_lerpTime = 0.0f;
				_state = LegState.Move;
			}
			else if(_state == LegState.Move)
			{
				MoveLeg();
			}
		}
		else if(_behaviorState == BehaviorState.Impale)
		{
			_shadowPos = Vector2.Lerp(_startPoint, _shadowIntermediatePoint, _lerpTime / _shadowMoveTime);
			_lerpTime += (Time.deltaTime* StaticData.t_scale);
		}
		else if(_behaviorState == BehaviorState.ApplyingBuff)
		{
			//do nothing for now
		}


		HandleStats();
	}

	Vector3 GetMoveVector()
	{
		return (Vector3)_bodyScript.GetComponent<BehaviorBlackboard>().moveDirection * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
		//return _bodyScript.GetComponent<BodyScript>().move_vec * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
	}

	//returns the midpoint of ab and sets it up a bit
	Vector3 GetIntermediatePoint(Vector3 a, Vector3 b, int upOffset)
	{
		float xPoint = a.x + ((b.x - a.x)/2);
		float yPoint = a.y + ((b.y - a.y)/2);
		float zPoint = a.z + ((b.z - a.z)/2);
		//_shadowIntermediatePoint = new Vector3(xPoint, yPoint, zPoint);

		if(upOffset == 1)
		{
			yPoint += _radius * 1.6f;
		}
		else if(upOffset == -1)
		{
			yPoint -= _radius;
		}

		if(upOffset != 0)
		{
			if(_blackboard.moveDirection.y > 0.5f)
			{
				yPoint = b.y + _radius;
			}
			else if(_blackboard.moveDirection.y < -0.5f)
			{
				yPoint = a.y + _radius;
			}
		}
		return new Vector3(xPoint, yPoint, zPoint);
	}

	//function that moves the leg and sets the leg to idle when done
	void MoveLeg()
	{
		if(_intermediatePointReached == false)
		{
			if( Vector2.Distance(transform.position, _intermediatePoint) < 0.01f)
			{
				_intermediatePointReached = true;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector3.Lerp(_startPoint, _intermediatePoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_shadowStartPoint, _shadowIntermediatePoint, _lerpTime / _moveTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
		else
		{
			if( Vector2.Distance((Vector2)transform.position, _targetPoint) < 0.01f)
			{
				AttackSystem.hitCircle((Vector2)transform.position, 0.4f, 5.0f, -1);
				_state = LegState.Idle;
			}
			else
			{
				transform.position = Vector3.Lerp(_intermediatePoint, _targetPoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_shadowIntermediatePoint, _shadowTargetPoint, _lerpTime / _moveTime);
				_lerpTime += (Time.deltaTime* StaticData.t_scale);
			}
		}
	}

	//this handles the logic for the stats on the boss like health and buffs
	void HandleStats()
	{
		if ( currentColor.duration > 0.0f )
		{
			if ( currentColor.timer >= currentColor.duration )
			{
				currentColor.duration = 0.0f;
				transform.parent.GetComponent<SpriteRenderer>().color = getResetColor ();
			}
			else
			{
				transform.parent.GetComponent<SpriteRenderer>().color = currentColor.color;
			}
			currentColor.timer += Time.deltaTime * StaticData.t_scale;
		}

		if (_behaviorState != BehaviorState.Disabled)
		{
			if(_currentHP <= 0.0f)
			{
				_behaviorState = BehaviorState.Disabled;
				_state = LegState.CalculateMove;
				_shadowStartPoint = transform.position;

				Vector3 Legscale = transform.parent.transform.localScale;
				Legscale.x = 2.0f;
				Legscale.y = 1.5f;
				transform.parent.transform.localScale = Legscale;
				//_disabled = true;
			}
		}
		else if(_behaviorState == BehaviorState.Disabled)
		{
			if(_currentHP == _maxHP)
			{
				_recovering = true;

				Vector3 Legscale = transform.parent.transform.localScale;
				Legscale.x = 2.25f;
				Legscale.y = 2.25f;
				transform.parent.transform.localScale = Legscale;
				_behaviorState = BehaviorState.Walking;
			}
			else if(_currentHP < _maxHP)
			{
				if(_blackboard.body._behaviorState != BodyScript.BehaviorState.Disabled)
				{
					//Removed passive regeneration from the legs.
					//TODO: Make it so that legs heal to full when the boss gets back up from being  vulnerable.
					//_currentHP += Mathf.Min(_regenRate * Time.deltaTime * StaticData.t_scale, _maxHP - _currentHP);
				}
			}
		}
	}

	public void ApplyBuff(int buffID)
	{
		_buffState = (BuffState)buffID;
		if(_buffState == BuffState.venom)
		{
			transform.parent.GetComponent<SpriteRenderer>().color = Color.green;
			_poisonDPS = 1.0f;
			_poisonDuration = 5.0f;
		}
		else if(_buffState == BuffState.web)
		{
			transform.parent.GetComponent<SpriteRenderer>().color = Color.blue;
			_currentWebbingHP = _maxWebbingHP;
		}

	}
	void RemoveBuff()
	{
		_buffState = BuffState.unbuffed;
		//reset vars (bookkeeping)
		_currentWebbingHP = 0.0f;
		_poisonDPS = 0.0f;
		_poisonDuration = 0.0f;
		transform.parent.GetComponent<SpriteRenderer>().color = Color.white;
	}

	public bool isPoisonous()
	{
		//returns whether or not the leg deals extra poison damage.
		return (_currentBuffDuration > 0.0f && _poisonDPS > 0.0f);
	}

	public Color getResetColor()
	{
		//returns the color the leg should be, given it's current state
		if ( _currentWebbingHP > 0.0f )
		{
			return new Color( 0.0f, 0.0f, 1.0f );
		}
		else if ( isPoisonous () )
		{
			return new Color( 0.0f, 1.0f, 0.0f );
		}
		else
		{
			return new Color( 1.0f, 1.0f, 1.0f );
		}
	}

	public void Hurt( float damage, int id )
	{
		//Handle players doing damage to the leg.
		//TODO: flash on taking HP damage.
		//TODO: sound.
		if ( invincible ) { return; } //immune to damage
		if ( _currentHP <= 0.0f ) { return; } //already dead
		//deal damage.

		//flash red.
		currentColor.color = new Color( 1.0f, 0.5f, 0.5f );
		currentColor.duration = 0.10f;
		currentColor.timer = 0.0f;

		if ( _currentWebbingHP > 0.0f )
		{
			//Redirect 90% to the armor
			float redirectionPercent = 0.90f;
			_currentWebbingHP -= damage * redirectionPercent;
			_currentHP -= damage * (1.0f - redirectionPercent);
			ScoreManager.DealtDamage( id, damage );
			//ScoreManager.DealtDamage ( id, damage * (1.0f - redirectionPercent) ); //if armor damage doesn't give score
			//Edge case check: if armor was broken, deal the excess damage
			if ( _currentWebbingHP < 0.0f )
			{
				_currentHP += _currentWebbingHP;
				_currentWebbingHP = 0.0f;
				//ScoreManager.DealtDamage ( id, _currentWebbingHP ); //if armor damage doesn't give score
				ScoreManager.BrokeArmor( id );
			}
		}
		else
		{
			//Normal damage
			_currentHP -= damage;
		}
		#region player score
		if ( id >= 0 && id < 4 )
		{
			//Give the player score ~ damage
			ScoreManager.DealtDamage( id, damage );
			//Give the player bonus score for killing blow
			if ( _currentHP <= 0.0f )
			{
				ScoreManager.KilledLeg( id );
			}
		}
		#endregion
	}
}
