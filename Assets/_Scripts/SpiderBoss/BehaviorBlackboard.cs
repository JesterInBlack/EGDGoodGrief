using UnityEngine;
using System.Collections;

//The blackboard allows tasks to chare data with each other.
public class BehaviorBlackboard : MonoBehaviour 
{
	/*
	public Transform P1 { get { return p1; } set { p1 = value; } }
	private Transform p1;

	public Transform P2 { get { return p2; } set { p2 = value; } }
	private Transform p2;

	public Transform P3 { get { return p3; } set { p3 = value; } }
	private Transform p3;

	public Transform P4 { get { return p4; } set { p4 = value; } }
	private Transform p4;
	*/

	public GameObject rightLeg1;
	public GameObject rightLeg2;
	public GameObject rightLeg3;
	public GameObject rightLeg4;
	public GameObject leftLeg5;
	public GameObject leftLeg6;
	public GameObject leftLeg7;
	public GameObject leftLeg8;

	public GameObject point1;
	public GameObject point2;
	public GameObject point3;
	public GameObject point4;
	public GameObject point5;
	public GameObject point6;
	public GameObject point7;
	public GameObject point8;

	public GameObject[] legsList = new GameObject[8];
	public GameObject[] points = new GameObject[8];

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
	public GameObject selectedLeg;
	public GameObject selectedPoint;

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
	}

}
