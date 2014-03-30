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
	private BehaviorData testBehavior2;
	private BehaviorData testBehavior3;

	//this is a list of all the actions the boss knows. It iterates through this to find the moves it can use currently.
	private List<BehaviorData> _behaviorList = new List<BehaviorData>(); 
	private int selectedIndex;

	private BehaviorManager _behaviorManager;

	//this is just a dummy variable to see what behavior is running in the inspector window
	//for debug purposes only
	public string _currentBehavior;

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

			if(allBehaviors[i].behaviorName == "Impale")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 35);
				_behaviorList.Add(testBehavior);
			}
			/*
			if(allBehaviors[i].behaviorName == "ApplyLegBuff")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 35);
				_behaviorList.Add(testBehavior);
			}
			/*
			if(allBehaviors[i].behaviorName == "MegaFlareUnity")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 35);
				_behaviorList.Add(testBehavior);
			}
			/*
			else if(allBehaviors[i].behaviorName == "TestBehavior")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 35);
				_behaviorList.Add(testBehavior);
			}
			else if(allBehaviors[i].behaviorName == "TestBehavior2")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 30);
				_behaviorList.Add(testBehavior);
			}
			else if(allBehaviors[i].behaviorName == "TestBehavior3")
			{
				testBehavior = new BehaviorData(allBehaviors[i], 1f, -1f, 1f, -1f, 23);
				_behaviorList.Add(testBehavior);
			}
			*/

		}
		#endregion

		#region GameState Variables
		for(int i = 0; i < GameState.playerThreats.Length; i++)
		{
			GameState.playerThreats[i] = 0.0f;
		}

		GameState.cooperationAxis = 0.0f; //initialized at 0
		GameState.angerAxis = -1.0f; //initialized at -1
		#endregion

		#region Blackboard Variables
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_blackboard.coopAxisRecovery = 0.01f;
		_blackboard.angerAxisRecovery = 0.01f;

		_blackboard.decisionState = BehaviorBlackboard.DecisionState.resetAllBehaviors;

		_blackboard.moveDirection = new Vector2(0, 0);
		#endregion
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool everyoneDead = true;
		//check health status of all the players
		foreach(GameObject player in GameState.players)
		{
			if(player.GetComponent<Player>().HP > 0)
			{
				everyoneDead = false;
			}
		}

		if(everyoneDead == false)
		{
			UpdateValences();
			UpdateBehaviors();

			//update the behaviorlist, they aren't monobehaviors so this needs to be done manually
			for(int i = 0; i < _behaviorList.Count; i++)
			{
				_behaviorList[i].Update();
			}
		}
		else
		{
			Debug.Log("Everyone dead");
			for(int i = 0; i < _behaviorList.Count; i++)
			{
				_behaviorManager.disableBehavior(_behaviorList[i].Action);
			}
		}
	}

	void UpdateValences()
	{
		//update the valences to their equilibriums
		if(GameState.cooperationAxis > 0.0f)
		{
			GameState.cooperationAxis -= Mathf.Min( (Time.deltaTime * _blackboard.coopAxisRecovery), Mathf.Abs(GameState.cooperationAxis));
		}
		else if(GameState.cooperationAxis < 0.0f)
		{
			GameState.cooperationAxis += Mathf.Min( (Time.deltaTime * _blackboard.coopAxisRecovery), Mathf.Abs(GameState.cooperationAxis));
		}
		if(GameState.angerAxis < 0.0f)
		{
			GameState.angerAxis += Mathf.Min( (Time.deltaTime * _blackboard.angerAxisRecovery), Mathf.Abs(GameState.angerAxis));
		}
	}

	void UpdateBehaviors()
	{
		if(_blackboard.decisionState == BehaviorBlackboard.DecisionState.resetAllBehaviors)
		{
			//loops all the behaviors into the list and disables them at the start
			for(int i = 0; i < _behaviorList.Count; i++)
			{
				if(_behaviorManager.isBehaviorEnabled(_behaviorList[i].Action))
				{
					_behaviorManager.disableBehavior(_behaviorList[i].Action);
				}
			}
			_blackboard.decisionState = BehaviorBlackboard.DecisionState.needsNewTask;
		}
		
		//Select the behavior to run
		else if(_blackboard.decisionState == BehaviorBlackboard.DecisionState.needsNewTask)
		{
			float selectedPriority = 0.0f;
			for(int i = 0; i < _behaviorList.Count; i++)
			{
				//find all moves that can be used with the current mood
				if(_behaviorList[i].AngerUpperBound > GameState.angerAxis && GameState.angerAxis > _behaviorList[i].AngerLowerBound && 
				   _behaviorList[i].CoopUpperBound > GameState.cooperationAxis && GameState.cooperationAxis > _behaviorList[i].CoopLowerBound)
				{
					if(_behaviorList[i].Priority > selectedPriority)
					{
						selectedIndex = i;
						selectedPriority = _behaviorList[i].Priority;
					}
				}
			}
			
			//set the priority to 0 for use
			_behaviorList[selectedIndex].UseAction();
			
			//enable and start up the behavior
			_behaviorManager.enableBehavior(_behaviorList[selectedIndex].Action);
			_behaviorManager.restartBehavior(_behaviorList[selectedIndex].Action);
			
			//picked a task
			_blackboard.decisionState = BehaviorBlackboard.DecisionState.runningTask;
			
			//set the dummy variable to see it in the inspector
			_currentBehavior = _behaviorList[selectedIndex].Action.behaviorName;
		}
		
		else if(_blackboard.decisionState == BehaviorBlackboard.DecisionState.runningTask)
		{
			if(_behaviorManager.isBehaviorEnabled(_behaviorList[selectedIndex].Action) == false)
			{
				//Debug.Log("Finished a behavior");
				_blackboard.decisionState = BehaviorBlackboard.DecisionState.needsNewTask;
			}
		}

	}
}
