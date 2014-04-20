using UnityEngine;
using System.Collections;

public static class StaticData
{
	//A class to hold always available, single-instance data
	public static float t_scale = 1.0f; //time scaling factor	
	public static float bulletTimeDuration = 0.0f; //duration of the effect

	//Game options
	public const float playerMoveSpeed = 3.0f;
	public const float percentHPNeededToRevive = 0.5f;
	//public const float friendlyFire = false;
}
