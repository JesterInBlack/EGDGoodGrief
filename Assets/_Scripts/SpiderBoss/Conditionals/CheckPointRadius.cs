using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckPointRadius : Action
{
	public bool _getTarget;
	public GameObject _radiusPoint;
	public float _radiusValue;

	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_radiusPoint = GameObject.Find("BodyShadow");

		if(_radiusValue == 0.0f)
		{
			Debug.Log("WARNING: Radius value in CheckPointRadius task not set");
		}
	}

	public override void OnStart()
	{

	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{

		for(int j = 0; j < GameState.players.Length; j++)
		{
			//close to the target
			if(GameState.players[j].GetComponent<Player>().isDowned == false)
			{
				if(Vector2.Distance((Vector2)_radiusPoint.transform.position, (Vector2)GameState.players[j].transform.position) < _radiusValue)
				{
					if(_getTarget == true)
					{
						_blackboard.targetPlayer = GameState.players[j];
					}
					return TaskStatus.Success;
				}
			}
		}

		return TaskStatus.Failure;
	}
}
