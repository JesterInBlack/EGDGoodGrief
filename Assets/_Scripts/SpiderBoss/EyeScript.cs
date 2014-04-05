using UnityEngine;
using System.Collections;

public class EyeScript : MonoBehaviour 
{
	private BehaviorBlackboard _blackboard;

	public float _rotationSpeed = 75.0f;						//degrees per second
	public Vector3 _rotationVec = new Vector3(0, 0, 0);	//the current rotation vector
	public float _targetAngle;								//target angle in degrees

	public enum BehaviorStates
	{
		Idle = 0,
		ChaseBeam = 1,
		LookDown =2,
	}
	public BehaviorStates _behaviorState;

	// Use this for initialization
	void Start () 
	{
		// cache for quick lookup
		_blackboard = transform.parent.GetComponent<BehaviorBlackboard>();
		_behaviorState = BehaviorStates.Idle;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_behaviorState == BehaviorStates.Idle)
		{
			if(_blackboard.targetPlayer == null)
			{
				//Debug.Log("nutin going on");
			}
			else if(_blackboard.targetPlayer != null)
			{
				//Debug.Log("nutin going on");
				GetTargetAngle(_blackboard.targetPlayer.transform.position);
				RotateToTarget(_rotationSpeed);
			}
		}
		else if(_behaviorState == BehaviorStates.ChaseBeam)
		{
			//do nothing for now
		}
		else if(_behaviorState == BehaviorStates.LookDown)
		{
			GetTargetAngle(_blackboard.body._shadowPos);
			RotateToTarget(_rotationSpeed);
		}
	}

	public void GetTargetAngle(Vector3 targetPos)
	{
		float deltaX = targetPos.x - transform.position.x;
		float deltaY = targetPos.y - transform.position.y;
		
		float angleInDeg = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;

		_targetAngle = angleInDeg;
	}

	public void RotateToTarget(float rotSpeed)
	{
		/*
		if(_rotationVec.z != _targetAngle)
		{
			//float currentAngle = _rotationVec.z;
			if(_targetAngle < _rotationVec.z + 180.0f && _targetAngle > _rotationVec.z)
			{
				_rotationVec.z += Mathf.Min(rotSpeed * StaticData.t_scale, Mathf.Abs(_rotationVec.z - _targetAngle));
			}
			else if(_targetAngle > _rotationVec.z - 180.0f && _targetAngle < _rotationVec.z)
			{
				_rotationVec.z -= Mathf.Min(rotSpeed * StaticData.t_scale, Mathf.Abs(_rotationVec.z - _targetAngle));
			}
			
			transform.eulerAngles = _rotationVec;
		}
		*/

		_rotationVec.z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, _targetAngle, rotSpeed * Time.deltaTime * StaticData.t_scale);
		transform.eulerAngles = _rotationVec;
	}
}
