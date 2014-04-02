using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class CheckRadius : Action 
{
	public float _checkRadius;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_checkRadius = 2.0f;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		for(int i = 0; i < _blackboard.points.Length; i++)
		{
			if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
			{
				//close to the target
				if(Vector2.Distance((Vector2)_blackboard.points[i].transform.position, (Vector2)_blackboard.targetPlayer.transform.position) < _checkRadius)
				{
					_blackboard.selectedLeg = _blackboard.legsList[i];
					_blackboard.selectedPoint = _blackboard.points[i];
					return TaskStatus.Success;
				}
			}
		}
		return TaskStatus.Failure;
	}
}
