using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

[TaskIcon("{SkinColor}SequenceIcon.png")]
[TaskCategory("CompositesCustom")]
public class GetPlayerPriority_Sequence : Composite
{
	public int _playerNumber;

	public override float GetPriority()
	{
		return GameState.playerThreats[_playerNumber];
	}

	// The index of the child that is currently running or is about to run.
	private int currentChildIndex = 0;
	// The task status of the last child ran.
	private TaskStatus executionStatus = TaskStatus.Inactive;
	
	public override int CurrentChildIndex()
	{
		return currentChildIndex;
	}
	
	public override bool CanExecute()
	{
		// We can continue to execuate as long as we have children that haven't been executed and no child has returned failure.
		return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure;
	}
	
	public override void OnChildExecuted(TaskStatus childStatus)
	{
		// Increase the child index and update the execution status after a child has finished running.
		currentChildIndex++;
		executionStatus = childStatus;
	}
	
	public override void OnEnd()
	{
		// All of the children have run. Reset the variables back to their starting values.
		executionStatus = TaskStatus.Inactive;
		currentChildIndex = 0;
	}
}

