using UnityEngine;
using System.Collections;

public class LegScript : MonoBehaviour {

	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.

	public float _radius = 2.5f;
	public float _movementFlux = 0.75f;
	public float _moveTime;
	public GameObject _radiusPoint;
	public GameObject _bodyScript;

	[HideInInspector]
	public Vector2 _shadowPos;
	[HideInInspector]
	public Vector2 _shadowIntermediatePoint;
	[HideInInspector]
	public float _shadowMoveTime;

	private bool _intermediatePointReached;
	private Vector2 _intermediatePoint;
	private Vector2 _targetPoint;
	[HideInInspector]
	public Vector2 _startPoint;
	[HideInInspector]
	public float _lerpTime;

	//enums
	public enum BehaviorState
	{
		Walking = 0,
		Impale = 1,
		Rake = 2,
	}
	[HideInInspector]
	public BehaviorState _behaviorState;

	public enum LegState
	{
		Idle = 0,
		CalculateMove = 1,
		Move = 2,
	}
	[HideInInspector]
	public LegState _state;

	// Use this for initialization
	void Start () 
	{
		_behaviorState = BehaviorState.Walking;
		_state = LegState.Idle;
		_moveTime = 0.15f;
	}
	
	// Update is called once per frame
	void Update () 
	{

		if(_behaviorState == BehaviorState.Walking)
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
				_targetPoint = (Vector2)_radiusPoint.transform.position + GetMoveVector();
				_intermediatePoint = GetIntermediatePoint(transform.position, _targetPoint);
				_intermediatePointReached = false;
				_startPoint = (Vector2)transform.position;
				_lerpTime = 0.0f;
				_state = LegState.Move;
			}
			else if(_state == LegState.Move)
			{
				//transform.position = _targetPoint;
				//_state = LegState.Idle;
				MoveLeg();
			}
		}
		else if(_behaviorState == BehaviorState.Impale)
		{
			_shadowPos = Vector2.Lerp(_startPoint, _shadowIntermediatePoint, _lerpTime / _shadowMoveTime);
			_lerpTime += Time.deltaTime;
		}

		/*
		// Cache the horizontal input.
		float h = Input.GetAxis("Horizontal");
		
		// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		if(h * rigidbody2D.velocity.x < maxSpeed)
		{
			// ... add a force to the player.
			rigidbody2D.AddForce(Vector2.right * h * moveForce);
		}
		
		// If the player's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
		{
			// ... set the player's velocity to the maxSpeed in the x axis.
			rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);
		}

		float v = Input.GetAxis("Vertical");
		// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		if(v * rigidbody2D.velocity.y < maxSpeed)
		{
			// ... add a force to the player.
			rigidbody2D.AddForce(Vector2.up * v * moveForce);
		}
		
		// If the player's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(rigidbody2D.velocity.y) > maxSpeed)
		{
			// ... set the player's velocity to the maxSpeed in the x axis.
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Sign(rigidbody2D.velocity.y) * maxSpeed);
		}
		*/
	}

	Vector2 GetMoveVector()
	{
		return _bodyScript.GetComponent<BehaviorBlackboard>().moveDirection * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
		//return _bodyScript.GetComponent<BodyScript>().move_vec * (_radius + _movementFlux/2 + Random.Range(-_movementFlux, 0));
	}

	//returns the midpoint of ab and sets it up a bit
	Vector2 GetIntermediatePoint(Vector2 a, Vector2 b)
	{
		float xPoint = a.x + ((b.x - a.x)/2);
		float yPoint = a.y + ((b.y - a.y)/2);
		_shadowIntermediatePoint = new Vector2(xPoint, yPoint);

		yPoint += _radius * 1.6f;

		if(_bodyScript.GetComponent<BodyScript>().move_vec.y > 0.5f)
		{
			yPoint = b.y + _radius;
		}
		else if(_bodyScript.GetComponent<BodyScript>().move_vec.y < -0.5f)
		{
			yPoint = a.y + _radius;
		}
		return new Vector2(xPoint, yPoint);
	}

	//function that moves the leg and sets the leg to idle when done
	void MoveLeg()
	{
		if(_intermediatePointReached == false)
		{
			if( Vector2.Distance((Vector2)transform.position, _intermediatePoint) < 0.01f)
			{
				_intermediatePointReached = true;
				_lerpTime = 0.0f;
			}
			else
			{
				transform.position = Vector2.Lerp(_startPoint, _intermediatePoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_startPoint, _shadowIntermediatePoint, _lerpTime / _moveTime);
				_lerpTime += Time.deltaTime;
			}
		}
		else
		{
			if( Vector2.Distance((Vector2)transform.position, _targetPoint) < 0.01f)
			{
				_state = LegState.Idle;
			}
			else
			{
				transform.position = Vector2.Lerp(_intermediatePoint, _targetPoint, _lerpTime / _moveTime);
				_shadowPos = Vector2.Lerp(_shadowIntermediatePoint, _targetPoint, _lerpTime / _moveTime);
				_lerpTime += Time.deltaTime;
			}
		}
	}
}
