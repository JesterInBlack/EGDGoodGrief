using UnityEngine;
using System.Collections;

public class Push
{
	//A class to push players around without stunning them.
	#region vars
	private Vector2 displacement;  //how far you move over the entire duration
	private float duration;        //over how long it moves
	private float timer;           //progress measurement

	public bool taggedForRemoval;
	#endregion

	public Push ( Vector2 pDisplacement, float pDuration )
	{
		displacement = pDisplacement;
		duration = pDuration;
		taggedForRemoval = false;
	}

	// Update is called once per frame
	public void Update ( GameObject me, float dt ) 
	{
		timer += dt;
		float dx = displacement.x / duration * dt;
		float dy = displacement.y / duration * dt;
		me.GetComponent<CustomController>().MoveNaoPlz ( new Vector3(dx, dy, 0.0f) );
		if ( timer >= duration )
		{
			taggedForRemoval = true;
		}
	}
}
