using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;

public class BehaviorData 
{
	//The behavior attached to this structure
	public Behavior Action { get { return _action; } }
	private Behavior _action;

	//variables that determine where on the axes this action lies
	public float AngerUpperBound { get { return _angerUpperBound; } }
	private float _angerUpperBound;

	public float AngerLowerBound { get { return _angerLowerBound; } }
	private float _angerLowerBound;

	public float CoopUpperBound { get { return _coopUpperBound; } }
	private float _coopUpperBound;

	public float CoopLowerBound { get { return _coopLowerBound; } }
	private float _coopLowerBound;

	// move priority values
	// when used value gets set to 0
	// regenerates up to a maximum priority value
	public float Priority { get { return _priority; } }
	private float _priority;
	private float _maxPriority;
	private float _priorityRecovery; //if at 1, it recovers at a rate of 1 per second

	//values for when boss succeeds/fails to use the move
	//uses a reference to the BehaviorBlackboard to changes it's values
	public float SuccessAngerDelta { get { return _successAngerDelta; } }
	private float _successAngerDelta;

	public float SuccessCoopDelta { get { return _successCoopDelta; } }
	private float _successCoopDelta;

	public float FailAngerDelta { get { return _failAngerDelta; } }
	private float _failAngerDelta;

	public float FailCoopDelta { get { return _failCoopDelta; } }
	private float _failCoopDelta;

	public BehaviorData(Behavior action, float aub, float alb, float cub, float clb, float priorityMax, float priorityRec = 1, float sad = 0, float scd = 0, float fad = 0, float fcd = 0)
	{
		_action = action;
		_angerUpperBound = aub;
		_angerLowerBound = alb;
		_coopUpperBound = cub;
		_coopLowerBound = clb;
		_maxPriority = priorityMax;
		_priority = priorityMax;
		_priorityRecovery = priorityRec;
		_successAngerDelta = sad;
		_successCoopDelta = scd;
		_failAngerDelta = fad;
		_failCoopDelta = fcd;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	public void Update () 
	{
		if(_priority < _maxPriority)
		{
			_priority += Mathf.Min( (Time.deltaTime * _priorityRecovery), Mathf.Abs(_maxPriority - _priority) );
		}

	}

	public void UseAction()
	{
		_priority = 0.0f;
	}
}
