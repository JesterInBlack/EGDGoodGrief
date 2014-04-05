using UnityEngine;
using System.Collections;

//The blackboard allows tasks to chare data with each other.
public class BehaviorBlackboard : MonoBehaviour 
{
	#region gameobject vars
	[HideInInspector]
	public LegScript rightLeg1;
	[HideInInspector]
	public LegScript rightLeg2;
	[HideInInspector]
	public LegScript rightLeg3;
	[HideInInspector]
	public LegScript rightLeg4;
	[HideInInspector]
	public LegScript leftLeg5;
	[HideInInspector]
	public LegScript leftLeg6;
	[HideInInspector]
	public LegScript leftLeg7;
	[HideInInspector]
	public LegScript leftLeg8;

	[HideInInspector]
	public GameObject point1;
	[HideInInspector]
	public GameObject point2;
	[HideInInspector]
	public GameObject point3;
	[HideInInspector]
	public GameObject point4;
	[HideInInspector]
	public GameObject point5;
	[HideInInspector]
	public GameObject point6;
	[HideInInspector]
	public GameObject point7;
	[HideInInspector]
	public GameObject point8;

	[HideInInspector]
	public GameObject disablePoint1;
	[HideInInspector]
	public GameObject disablePoint2;
	[HideInInspector]
	public GameObject disablePoint3;
	[HideInInspector]
	public GameObject disablePoint4;
	[HideInInspector]
	public GameObject disablePoint5;
	[HideInInspector]
	public GameObject disablePoint6;
	[HideInInspector]
	public GameObject disablePoint7;
	[HideInInspector]
	public GameObject disablePoint8;

	[HideInInspector]
	public GameObject rightMaw;
	[HideInInspector]
	public GameObject leftMaw;
	[HideInInspector]
	public GameObject eye;
	[HideInInspector]
	public BodyScript body;
	#endregion

	public LegScript[] legsList = new LegScript[8];
	public GameObject[] points = new GameObject[8];
	public GameObject[] disablePoints = new GameObject[8];

	public float coopAxisRecovery;
	public float angerAxisRecovery;

	//public bool needsNewTask;
	public enum DecisionState
	{
		resetAllBehaviors = 0,
		needsNewTask = 1,
		runningTask = 2,
	}
	[HideInInspector]
	public DecisionState decisionState;

	public GameObject targetPlayer;

	//movement vars
	public Vector2 moveDirection;

	//impale vars
	public LegScript selectedLeg;
	public GameObject selectedPoint;

	public BehaviorData _currentBehavior;
	public bool attackPatternStopped;

	void Start()
	{
		points[0] = point1;
		points[1] = point2;
		points[2] = point3;
		points[3] = point4;
		points[4] = point5;
		points[5] = point6;
		points[6] = point7;
		points[7] = point8;

		rightLeg1.GetComponent<LegScript>()._id = 0;
		legsList[0] = rightLeg1;
		rightLeg2.GetComponent<LegScript>()._id = 1;
		legsList[1] = rightLeg2;
		rightLeg3.GetComponent<LegScript>()._id = 2;
		legsList[2] = rightLeg3;
		rightLeg4.GetComponent<LegScript>()._id = 3;
		legsList[3] = rightLeg4;

		leftLeg5.GetComponent<LegScript>()._id = 4;
		legsList[4] = leftLeg5;
		leftLeg6.GetComponent<LegScript>()._id = 5;
		legsList[5] = leftLeg6;
		leftLeg7.GetComponent<LegScript>()._id = 6;
		legsList[6] = leftLeg7;
		leftLeg8.GetComponent<LegScript>()._id = 7;
		legsList[7] = leftLeg8;

		disablePoints[0] = disablePoint1;
		disablePoints[1] = disablePoint2;
		disablePoints[2] = disablePoint3;
		disablePoints[3] = disablePoint4;
		disablePoints[4] = disablePoint5;
		disablePoints[5] = disablePoint6;
		disablePoints[6] = disablePoint7;
		disablePoints[7] = disablePoint8;

	}

}
