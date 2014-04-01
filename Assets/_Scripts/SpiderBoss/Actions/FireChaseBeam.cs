using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Attack")]
public class FireChaseBeam : Action
{
	public float _chaseSpeed = 45.0f;

	public float _chaseDuration = 10.0f;
	public float _chaseTime;

	private Player _targetPlayer;
	private EyeScript _eyesScript;
	private BehaviorBlackboard _blackboard;
	private bool _noTargetsLeft;
	
	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		_eyesScript = _blackboard.eye.GetComponent<EyeScript>();
	}

	public override void OnStart()
	{
		_targetPlayer = _blackboard.targetPlayer.GetComponent<Player>();
		_eyesScript._behaviorState = EyeScript.BehaviorStates.chaseBeam;
		_chaseTime = 0.0f;
		_noTargetsLeft = false;
	}
	
	//runs the actual task
	public override TaskStatus OnUpdate()
	{
		if(_noTargetsLeft == false)
		{
			if(_chaseTime >= _chaseDuration)
			{
				return TaskStatus.Success;
			}
			else
			{
				_chaseTime += Time.deltaTime * StaticData.t_scale;

				if(_targetPlayer.isDowned == false)
				{
					_eyesScript.GetTargetAngle(_targetPlayer.transform.position);
					_eyesScript.RotateToTarget(_chaseSpeed);
				}
				//find a the next target and keep going!
				else
				{
					//Debug.Log("Switching target!");
					GetNewTarget();
				}
				return TaskStatus.Running;
			}
		}
		else
		{
			return TaskStatus.Failure;
		}
	}

	void GetNewTarget()
	{
		float highestAggro = 0;
		int aggroIndex = -1;
		for(int i = 0; i < GameState.players.Length; i++)
		{
			if(GameState.players[i].GetComponent<Player>().isDowned == false && GameState.playerThreats[i] >= highestAggro)
			{
				highestAggro = GameState.playerThreats[i];
				aggroIndex = i;
			}
		}
		
		if(aggroIndex == -1)
		{
			_noTargetsLeft = true;
		}
		else
		{
			//Debug.Log("new target is player: " + aggroIndex);
			_targetPlayer = GameState.players[aggroIndex].GetComponent<Player>();
		}
	}

	public override void OnEnd()
	{
		_eyesScript._behaviorState = EyeScript.BehaviorStates.idle;
	}
}
