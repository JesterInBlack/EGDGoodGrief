using UnityEngine;
using System.Collections;
using BehaviorDesigner.Runtime.Tasks;

public class LegListSelector : Action
{
	private BehaviorBlackboard _blackboard;

	[SharedField]
	public int[] _selectedLegs = new int[4];

	public override void OnAwake()
	{
		// cache for quick lookup
		_blackboard = gameObject.GetComponent<BehaviorBlackboard>();
		for(int i = 0; i < _selectedLegs.Length; i++)
		{
			_selectedLegs[i] = -1;
		}
	}


	public override TaskStatus OnUpdate()
	{
		//find at least 2 legs from the right legs
		int limiter = 0;
		int count = 0;
		for(int i = 0; i < 4; i++)
		{
			//find at least 2 on the right side
			if(limiter < 2)
			{
				if(_blackboard.legsList[i].GetComponent<LegScript>()._currentHP > 0)
				{
					_selectedLegs[count] = i;
					count++;
				}
			}
		}

		//if there's only one suitable leg we found
		//we need to find 3 on the left side, otherwise failure
		if(count == 1 || count == 2)
		{
			for(int i = 4; i < 8; i++)
			{
				if(_blackboard.legsList[i].GetComponent<LegScript>()._currentHP > 0)
				{
					_selectedLegs[count] = i;
					count++;
				}
			}

			//did you get all 4 legs?
			if(count == 4)
			{
				return TaskStatus.Success;
			}
			else
			{
				return TaskStatus.Failure;
			}
		}
		//you found 2, so try to find 2 on the left side
		else if(count == 2)
		{
			for(int i = 4; i < 8; i++)
			{
				if(_blackboard.legsList[i].GetComponent<LegScript>()._currentHP > 0)
				{
					_selectedLegs[count] = i;
					count++;
				}
			}
			//did you get all 4 legs?
			if(count == 4)
			{
				return TaskStatus.Success;
			}
			//if not, check the right side again for a 4th one if you only have 3
			else if(count == 3)
			{
				for(int i = 0; i < 4; i++)
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
						if(_blackboard.legsList[i].GetComponent<LegScript>()._currentHP > 0)
						{
							_selectedLegs[count] = i;
							count++;
						}
					}
				}
				if(count == 4)
				{
					return TaskStatus.Success;
				}
				else
				{
					return TaskStatus.Failure;
				}
			}
			else
			{
				return TaskStatus.Failure;
			}
		}

		return TaskStatus.Failure;
	}
}
