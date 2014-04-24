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

		if(anger < -0.25f)
		{
			_minimumAttacks.Value = 1;
		}
		else if(anger < 0.25f)
		{
			_minimumAttacks.Value = 2;
		}
		else if(anger < 0.75f)
		{
			_minimumAttacks.Value = 3;
		}
		else
		{
			_minimumAttacks.Value = 4;
		}
		return TaskStatus.Success;
	}
}
