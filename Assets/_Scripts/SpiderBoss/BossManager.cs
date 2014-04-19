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
	//private BehaviorData testBehavior2;
	//private BehaviorData testBehavior3;

	//this is a list of all the actions the boss knows. It iterates through this to find the moves it can use currently.
	private List<BehaviorData> _behaviorList = new List<BehaviorData>(); 
	private int selectedIndex;

	private BehaviorManager _behaviorManager;

	//this is just a dummy variable to see what behavior is running in the inspector window
	//for debug purposes only
	public int _currentBehavior;
	public float _coopAxis;
	public float _angerAxis;
	private bool _foundMove;

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
		BehaviorTree[] allBehaviors = FindObjectsOfType(typeof(BehaviorTree)) as BehaviorTree[];

		//Debug.Log(allBehaviors.Length);
		for(int i = 0; i < allBehaviors.Length; i++)
		{

			if(allBehaviors[i].group == 1)	//Impale
			{
				testBehavior = new BehaviorData(allBehaviors[i], -1f, 1f, -1f, 1f, 55.0f, 0.75f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 2)	//apply buff
			{
				testBehavior = new BehaviorData(allBehaviors[i], -0.5f, 1f, -0.5f, 0.5f, 25.0f, 0.75f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 3) //AoeVenom
			{
				testBehavior = new BehaviorData(allBehaviors[i], -1f, 1f, -0.5f, 1f, 30.0f, 1.2f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 4) //AoEWeb
			{
				testBehavior = new BehaviorData(allBehaviors[i], -1f, 1f, -1f, 0.4f, 25.0f, 1.2f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 5) //ChaseBeam
			{
				testBehavior = new BehaviorData(allBehaviors[i], -1f, 0.0f, -0.1f, 1f, 45.0f, 0.5f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 6) //UnityMegaFlare
			{
				testBehavior = new BehaviorData(allBehaviors[i], -1f, -0.5f, 0.5f, 1f, 60.0f, 0.5f, -0.1f, 0.25f, 0.0f, 0.0f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 7) //Dissention Suction
			{
				testBehavior = new BehaviorData(allBehaviors[i], 0.3f, 1f, 0.4f, 1f, 60.0f, 0.5f, -0.1f, -0.25f, 0.0f, 0.0f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 8) //PointLaser
			{
				testBehavior = new BehaviorData(allBehaviors[i], -0.3f, 1f, 0f, 1f, 45.0f, 1.3f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 9) //WebTether
			{
				testBehavior = new BehaviorData(allBehaviors[i], -0.5f, 0.5f, 0.5f, 1f, 50.0f, 0.6f, -0.2f, 0f, 0.0f, 0.0f);
				_behaviorList.Add(testBehavior);
			}
			if(allBehaviors[i].group == 10) //DissentionEggSac
			{
				testBehavior = new BehaviorData(allBehaviors[i], 0.3f, 1f, 0.4f, 1f, 65.0f, 0.5f, -0.1f, -0.25f, 0.0f, 0.0f);
				_behaviorList.Add(testBehavior);
			}
		}
		#endregion

		#region GameState Variables
		for(int i = 0; i < GameState.playerThreats.Length; i++)
		{
			GameState.playerThreats[i] = 0.0f;
		}

		GameState.cooperationAxis = 0.0f; //initialized at 0
		GameState.angerAxis = 0.0f; //initialized at -1
		#endregion

		#region Blackboard Variables
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

		_blackboard.coopAxisRecovery = 0.01f;
		_blackboard.angerAxisRecovery = 0.01f;

		_blackboard.decisionState = BehaviorBlackboard.DecisionState.resetAllBehaviors;

		_blackboard.moveDirection = new Vector2(0, 0);
		_blackboard.attackPatternStopped = false;

		_blackboard._invincible = true;
		_blackboard._moveToEndScreen = false;

		_blackboard._naturalThreatRecovery = 1.25f;
		_blackboard._attackWasSuccess = false;
		#endregion

		_foundMove = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		bool everyoneDead = true;
		//check health status of all the players
		foreach(GameObject player in GameState.players)
		{
			if(player.GetComponent<Player>().isDowned == false)
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
			//Debug.Log("Everyone dead");
			for(int i = 0; i < _behaviorList.Count; i++)
			{
				_behaviorManager.disableBehavior(_behaviorList[i].Action);
			}
		}

		_coopAxis = GameState.cooperationAxis;
		_angerAxis = GameState.angerAxis;
	}

	void UpdateValences()
	{
		//update the valences to their equilibriums
		if(GameState.cooperationAxis > 0.0f)
		{
			GameState.cooperationAxis -= Mathf.Min(( (Time.deltaTime* StaticData.t_scale) * _blackboard.coopAxisRecovery), Mathf.Abs(GameState.cooperationAxis));
		}
		else if(GameState.cooperationAxis < 0.0f)
		{
			GameState.cooperationAxis += Mathf.Min(( (Time.deltaTime* StaticData.t_scale) * _blackboard.coopAxisRecovery), Mathf.Abs(GameState.cooperationAxis));
		}
		if(GameState.angerAxis < 0.0f)
		{
			GameState.angerAxis += Mathf.Min(( (Time.deltaTime* StaticData.t_scale) * _blackboard.angerAxisRecovery), Mathf.Abs(GameState.angerAxis));
		}
		else if(GameState.cooperationAxis > 0.0f)
		{
			GameState.angerAxis -= Mathf.Min(( (Time.deltaTime* StaticData.t_scale) * _blackboard.angerAxisRecovery/3.0f), Mathf.Abs(GameState.angerAxis));
		}

		GameState.angerAxis = Mathf.Clamp(GameState.angerAxis, -1.0f, 1.0f);
		GameState.cooperationAxis = Mathf.Clamp(GameState.cooperationAxis, -1.0f, 1.0f);

		for(int i= 0; i < GameState.playerThreats.Length; i++)
		{
			GameState.playerThreats[i] -= Mathf.Min(( (Time.deltaTime * StaticData.t_scale) * _blackboard._naturalThreatRecovery), GameState.playerThreats[i]);
			//Debug.Log("THREAT: " + GameState.playerThreats[i]);
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
				if(_behaviorList[i].AngerUpperBound >= GameState.angerAxis && GameState.angerAxis >= _behaviorList[i].AngerLowerBound && 
				   _behaviorList[i].CoopUpperBound >= GameState.cooperationAxis && GameState.cooperationAxis >= _behaviorList[i].CoopLowerBound)
				{
					if(_behaviorList[i].Priority > selectedPriority)
					{
						selectedIndex = i;
						selectedPriority = _behaviorList[i].Priority;
						_foundMove = true;
					}
				}
			}

			if(_foundMove == true)
			{
				//enable and start up the behavior
				_behaviorManager.enableBehavior(_behaviorList[selectedIndex].Action);
				_behaviorManager.restartBehavior(_behaviorList[selectedIndex].Action);
				
				//picked a task
				_blackboard.decisionState = BehaviorBlackboard.DecisionState.runningTask;
				
				//set the dummy variable to see it in the inspector
				_currentBehavior = _behaviorList[selectedIndex].Action.group;
				_blackboard._currentBehavior = _behaviorList[selectedIndex];
				_foundMove = false;
			}
			else
			{
				Debug.Log("WARNING NO MOVE FOUND IN THIS AREA");
			}
		}
		
		else if(_blackboard.decisionState == BehaviorBlackboard.DecisionState.runningTask)
		{
			if(_blackboard.attackPatternStopped == false)
			{
				if(_behaviorManager.isBehaviorEnabled(_behaviorList[selectedIndex].Action) == false)
				{
					//Debug.Log("Finished a behavior");
					//set the priority to 0 for use
					_behaviorList[selectedIndex].UseAction(_blackboard._attackWasSuccess);
					_blackboard._attackWasSuccess = false;

					_blackboard.decisionState = BehaviorBlackboard.DecisionState.needsNewTask;
					_blackboard._currentBehavior = null;
				}
			}
		}

	}
}
