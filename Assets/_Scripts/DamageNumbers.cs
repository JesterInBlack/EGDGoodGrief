using UnityEngine;
using System.Collections;

public class DamageNumbers : MonoBehaviour 
{
	//TODO: make this script get called by scoremanager
	//TODO: make this script call players show text for defensive. For offensive, place own text at location.
	//TODO: link to player.
	//TODO: record player taking / dealing / blocking damage over time. (3 separate timers) (reset on new damage)
	//TODO: improvment: 1 timer per attacker id (4 + 1, track independently)
	//TODO: if the timer runs out, put a damage number up on the screen.

	#region vars
	private const float takeDelay = 0.1f;   //how long, in seconds, needs to pass before an attack is considered separate.
	private const float dealDelay = 0.1f;   //how long, in seconds, needs to pass before an attack is considered separate.
	private const float blockDelay = 0.1f;  //how long, in seconds, needs to pass before an attack is considered separate.
	#endregion

	void Awake()
	{
		//player = GetComponent<Player>();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
