using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

public class LegListSelector : Action
{
	private BehaviorBlackboard _blackboard;
	
	public int[] _selectedLegs = new int[4];

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();

	}

	public override void OnStart()
	{
		for(int i = 0; i < _selectedLegs.Length; i++)
		{
			_selectedLegs[i] = -1;
		}
	}
	
	public override TaskStatus OnUpdate()
	{
		//find at least 2 legs from the right legs
		int count = 0;
		for(int i = 0; i < 4; i++)
		{
			//find at least 2 on the right side
			if(count < 2)
			{
				if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
				{
					_selectedLegs[count] = i;
					count++;
				}
			}
		}
		//Debug.Log("found " + count + " legs");

		//if there's only one suitable leg we found
		//we need to find 3 on the left side, otherwise failure
		if(count == 1)
		{
			for(int i = 4; i < 8; i++)
			{
				if(count < 4)
				{
					if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
					{
						_selectedLegs[count] = i;
						count++;
					}
				}
			}

			//did you get all 4 legs?
			if(count == 4)
			{
				//Debug.Log("should be a 1 - 3 spread");
				return TaskStatus.Success;
			}
			else
			{
				//Debug.Log("only 1 right leg, not enough left legs");
				return TaskStatus.Failure;
			}
		}
		//you found 2, so try to find 2 on the left side
		else if(count == 2)
		{
			for(int i = 4; i < 8; i++)
			{
				if(count < 4)
				{
					if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
					{
						_selectedLegs[count] = i;
						count++;
					}
				}
			}
			//did you get all 4 legs?
			if(count == 4)
			{
				//Debug.Log("should be a 2 - 2 spread");
				return TaskStatus.Success;
			}
			//if not, check the right side again for a 4th one if you only have 3
			else if(count == 3)
			{
				for(int i = 0; i < 4; i++)
				{
					if(count < 4)
					{
						bool toCheck = true;
						for(int j = 0; j < _selectedLegs.Length; j++)
						{
							if(_selectedLegs[j] == i)
							{
								toCheck = false;
							}
						}
						if(toCheck == true)
						{
							if(_blackboard.legsList[i]._behaviorState != LegScript.BehaviorState.Disabled)
							{
								_selectedLegs[count] = i;
								count++;
							}
						}
					}
				}
				if(count == 4)
				{
					//Debug.Log("should be a 3 - 1 spread");
					return TaskStatus.Success;
				}
				else
				{
					//Debug.Log("only 1 left leg, not enough for a 3 - 1 spread");
					return TaskStatus.Failure;
				}
			}
			else
			{
				return TaskStatus.Failure;
			}
		}

		//Debug.Log("this should only run if there are no right legs to pick");
		return TaskStatus.Failure;
	}

	public override void OnEnd()
	{
		_blackboard.selectedLegs = _selectedLegs;
	}
}
