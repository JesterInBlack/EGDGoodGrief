using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class CheckRadius : Action 
{
	/*
	public GameObject _point1;
	public GameObject _point2;
	public GameObject _point3;
	public GameObject _point4;
	public GameObject _point5;
	public GameObject _point6;
	public GameObject _point7;
	public GameObject _point8;
	*/
	public float _checkRadius;

	private GameObject[] _points = new GameObject[8];
	private BehaviorBlackboard _blackboard;

	public override void OnAwake()
	{
		/*
		_points[0] = _point1;
		_points[1] = _point2;
		_points[2] = _point3;
		_points[3] = _point4;
		_points[4] = _point5;
		_points[5] = _point6;
		_points[6] = _point7;
		_points[7] = _point8;
*/
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_checkRadius = 1.0f;
	}

	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		for(int i = 0; i < _points.Length; i++)
		{
			//close to the target
			if(Vector2.Distance((Vector2)_blackboard.points[i].transform.position, (Vector2)_blackboard.targetPlayer.transform.position) < _checkRadius)
			{
				_blackboard.selectedLeg = _blackboard.legsList[i];
				_blackboard.selectedPoint = _blackboard.points[i];
				return TaskStatus.Success;
			}
		}
		return TaskStatus.Failure;
	}
}
