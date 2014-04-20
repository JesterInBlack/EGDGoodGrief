using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckRadius : Action 
{
	public float _checkRadius;

	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_checkRadius = _blackboard.point1.GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		for (int i = 0; i < _blackboard.points.Length; i++)
		{
			if (_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
			{
				for (int j = 0; j < GameState.players.Length; j++)
				{
					//close to the target
					if ( GameState.players[j] != null )
					{
						if (GameState.players[j].GetComponent<Player>().isDowned == false)
						{
							if(Vector2.Distance((Vector2)_blackboard.points[i].transform.position, (Vector2)GameState.players[j].transform.position) < _checkRadius)
							{
								_blackboard.targetPlayer = GameState.players[j];
								_blackboard.selectedLeg = _blackboard.legsList[i];
								_blackboard.selectedPoint = _blackboard.points[i];
								return TaskStatus.Success;
							}
						}
					}
				}
			}
		}
		return TaskStatus.Failure;
	}
}
