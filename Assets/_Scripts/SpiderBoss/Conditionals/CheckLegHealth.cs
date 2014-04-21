using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckLegHealth : Action
{
	public int _legNumber;

	private BehaviorBlackboard _blackboard;
	private LegScript _legScript;

	public float _thresholdPercent = 0.0f;			//from 0.0 to 1.0
	public GameObject _leg;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_legScript = _blackboard.legsList[_legNumber];
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if((float)(_legScript._currentHP / _legScript._maxHP) > _thresholdPercent)
		{
			return TaskStatus.Success;
		}
		else
		{
			return TaskStatus.Failure;
		}
	}
}
