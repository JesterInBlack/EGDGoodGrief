using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;

public class BossManager : MonoBehaviour 
{	
	//This allows other functions to access the public functions from BossManager
	public static BossManager _instance;

	private BehaviorBlackboard _blackboard;

	//list all the behavior subtrees here
	private BehaviorData testBehavior;
	//private Behavior behaviors;
	//private Behavior behaviors;


	//this is a list of all the actions the boss knows. It iterates through this to find the moves it can use currently.
	private List<BehaviorData> _behaviorList = new List<BehaviorData>(); 
	//this is a list that gets populated with the actions the boss can currently take. It is sorted by priority.
	private List<BehaviorData> _actionList = new List<BehaviorData>();

	private BehaviorManager _behaviorManager;

	public void Awake()
	{
		_instance = this;
	}
	
	public void Start()
	{
		// cache for quick lookup
		_behaviorManager = BehaviorManager.instance;

		#region Behavior Initialization
		//instantiate all the BehaviorData
		Behavior[] allBehaviors = FindObjectsOfType(typeof(Behavior)) as Behavior[];

		for(int i = 0; i < allBehaviors.Length; i++)
		{
			if(allBehaviors[i].behaviorName == "TestBehavior")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 0.8f, -0.8f, 0.8f, -0.8f, 5);
				_behaviorList.Add(testBehavior);
			}
		}

		//adds all the behaviors into the list and disables them at the start
		for(int i = 0; i < _behaviorList.Count; i++)
		{
			if(_behaviorManager.isBehaviorEnabled(_behaviorList[i].Action))
			{
				_behaviorManager.disableBehavior(_behaviorList[i].Action);
			}
		}
		#endregion

		#region Blackboard Variables
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		//sets the 4 characters
		//for references to the plays themselves, use GameState.Players[]. ID = player number
		_blackboard.p1Threat = 0.0f;
		_blackboard.p2Threat = 0.0f;
		_blackboard.p3Threat = 0.0f;
		_blackboard.p4Threat = 0.0f;

		_blackboard.cooperationAxis = 0.0f;
		_blackboard.angerAxis = -1.0f;

		#endregion

	}
	
	// Update is called once per frame
	void Update () 
	{
		//update the valences to their equilibriums


		//do other things here


		//update the behaviorlist, they aren't monobehaviors so this needs to be done manually
		for(int i = 0; i < _behaviorList.Count; i++)
		{
			_behaviorList[i].Update();
		}
	}
}
