using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class ImpaleSetAttackCounter : Action
{
	public SharedInt _minimumAttacks;

	public override TaskStatus OnUpdate()
	{
		float anger = GameState.angerAxis;

		if(anger < -0.5f)
		{
			_minimumAttacks.Value = 1;
		}
		else if(anger < 0.0f)
		{
			_minimumAttacks.Value = 2;
		}
		else if(anger < 0.25f)
		{
			_minimumAttacks.Value = 3;
		}
		else if(anger < 0.7f)
		{
			_minimumAttacks.Value = 4;
		}
		else
		{
			_minimumAttacks.Value = 5;
		}
		return TaskStatus.Success;
	}
}
