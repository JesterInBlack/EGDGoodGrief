using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Logic")]
public class CheckLegRadius : Action
{
	public int _legNumber = 0;

	public bool _getTarget;
	public bool _getSharedTarget;
	public SharedGameObject _sharedTarget;
	public GameObject _radiusPoint;
	private float _radiusValue;
	
	private BehaviorBlackboard _blackboard;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_radiusPoint = _blackboard.points[_legNumber];
		_radiusValue = _radiusPoint.GetComponent<CircleCollider2D>().radius;
	}
	
	public override void OnStart()
	{
		
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		
		for (int j = 0; j < GameState.players.Length; j++)
		{
			if ( GameState.players[j] != null )
			{
				//close to the target
				if (GameState.players[j].GetComponent<Player>().isDowned == false)
				{
					if(Vector2.Distance((Vector2)_radiusPoint.transform.position, (Vector2)GameState.players[j].transform.position) < _radiusValue)
					{
						if(_getTarget == true)
						{
							_blackboard.targetPlayer = GameState.players[j];
						}
						if(_getSharedTarget == true)
						{
							_sharedTarget.Value = GameState.players[j];
						}
						return TaskStatus.Success;
					}
				}
			}
		}
		
		return TaskStatus.Failure;
	}
}
