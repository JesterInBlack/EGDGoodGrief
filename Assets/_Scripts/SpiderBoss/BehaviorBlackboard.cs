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

	public float coopAxisRecovery;
	public float angerAxisRecovery;

	//public bool needsNewTask;
	public enum DecisionState
	{
		needsNewTask = 1,
		runningTask = 2,
	}
	public DecisionState decisionState;

}
