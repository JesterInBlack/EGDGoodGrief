using UnityEngine;
using System.Collections;

public class BossDebugHelper : MonoBehaviour 
{
	#region vars
	GUIText myText;

	private float _coopAxisRecoveryHolder;
	private float _angerAxisRecoveryHolder;
	private bool _freezeAxes;
	private bool _debuggingEnabled;

	private bool _getRecoveries;
	#endregion

	// Use this for pre-initialization
	void Awake ()
	{
		myText = GetComponent<GUIText>();
	}

	// Use this for initialization
	void Start () 
	{
		_getRecoveries = false;
		_debuggingEnabled = false;
		_freezeAxes = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_getRecoveries == false)
		{
			_angerAxisRecoveryHolder = GameState.boss.GetComponent<BehaviorBlackboard>().angerAxisRecovery;
			_coopAxisRecoveryHolder = GameState.boss.GetComponent<BehaviorBlackboard>().coopAxisRecovery;
			_getRecoveries = true;
		}

		if(Input.GetKeyDown(KeyCode.D))
		{
			if(GetComponent<GUIText>().enabled == false)
			{
				GetComponent<GUIText>().enabled = true;
				_debuggingEnabled = true;
			}
			else
			{
				GetComponent<GUIText>().enabled = false;
				_debuggingEnabled = false;

				GameState.boss.GetComponent<BehaviorBlackboard>().angerAxisRecovery = _angerAxisRecoveryHolder;
				GameState.boss.GetComponent<BehaviorBlackboard>().coopAxisRecovery = _coopAxisRecoveryHolder;
				_freezeAxes = false;
			}

		}

		if(_debuggingEnabled == true)
		{
			//manipulate axes
			if(Input.GetKeyDown(KeyCode.F))
			{
				if(_freezeAxes == false)
				{
					GameState.boss.GetComponent<BehaviorBlackboard>().angerAxisRecovery = 0.0f;
					GameState.boss.GetComponent<BehaviorBlackboard>().coopAxisRecovery = 0.0f;
					_freezeAxes = true;
				}
				else
				{
					GameState.boss.GetComponent<BehaviorBlackboard>().angerAxisRecovery = _angerAxisRecoveryHolder;
					GameState.boss.GetComponent<BehaviorBlackboard>().coopAxisRecovery = _coopAxisRecoveryHolder;
					_freezeAxes = false;
				}
			}
			if(Input.GetKeyDown(KeyCode.UpArrow))
			{
				GameState.angerAxis += 0.1f;
			}
			if(Input.GetKeyDown(KeyCode.DownArrow))
			{
				GameState.angerAxis -= 0.1f;
			}
			if(Input.GetKeyDown(KeyCode.LeftArrow))
			{
				GameState.cooperationAxis -= 0.1f;
			}
			if(Input.GetKeyDown(KeyCode.RightArrow))
			{
				GameState.cooperationAxis += 0.1f;
			}
			
			//input to kill legs
			if(Input.GetKeyDown(KeyCode.Alpha1))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[0].GetComponent<LegScript>()._currentHP = 0;
			}if(Input.GetKeyDown(KeyCode.Alpha2))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[1].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha3))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[2].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha4))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[3].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha5))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[4].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha6))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[5].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha7))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[6].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.Alpha8))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().legsList[7].GetComponent<LegScript>()._currentHP = 0;
			}
			if(Input.GetKeyDown(KeyCode.K))
			{
				GameState.boss.GetComponent<BehaviorBlackboard>().HP = 0;
			}

		}

		string text = "Coop: " + GameState.cooperationAxis + 
			"\nAnger: " + GameState.angerAxis + 
			"\nThreat: " + 
			(int)GameState.playerThreats[0] + ", " + 
			(int)GameState.playerThreats[1] + ", " + 
			(int)GameState.playerThreats[2] + ", " + 
			(int)GameState.playerThreats[3] +
			"\nCurrentBehavior: " + GameState.boss.GetComponent<BossManager>()._currentBehavior;
		myText.text = text;
	}
}
