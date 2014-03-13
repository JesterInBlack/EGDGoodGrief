using UnityEngine;
using System.Collections;

public enum ItemType {ITEM_FAST, ITEM_CHARGE_AND_RELEASE, ITEM_AIM_RAY, ITEM_AIM_POINT};
//fast: = use on button press
//charge and release: = use on button release, time button hold down
//aim ray: = aim it at an angle (arrows)
//aim point: = aim it at a specific point (jars / thrown objects)

public class Item
{
	#region vars
	public string name;
	public float coolDownDelay = 0.0f;  //How long the item takes to cool down
	public float coolDownTimer = 0.0f;  //Current status ( 0.0f = ready )
	public ItemType itemType;
	#endregion

	//TODO: add an interface for calling functions?
	// Update is called once per frame (from the player class)
	public void Update ( float dt ) 
	{
		coolDownTimer = Mathf.Max ( coolDownTimer - dt, 0.0f );
	}

	//TODO: add to general input (player / controller)
	//add states for aim point, aim ray, fast use, charge
	//item windup, item aim point, item aim ray, item charge

	//button mash war? -> input (2 players, vars for buttons mashed?)
	//TODO: add interruption for charged / aiming?
	//TODO: add bool to disable that interruption (static settings class)
}
