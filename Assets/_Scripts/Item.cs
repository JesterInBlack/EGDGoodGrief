using UnityEngine;
using System.Collections;

public enum ItemType {ITEM_FAST, ITEM_CHARGE_AND_RELEASE, ITEM_AIM_RAY, ITEM_AIM_POINT};
//fast: = use on button press
//charge and release: = use on button release, time button hold down
//aim ray: = aim it at an angle (arrows)
//aim point: = aim it at a specific point (jars / thrown objects)

public enum ItemName {STOPWATCH, AURA_OFFENSE, AURA_DEFENSE, AURA_REGEN, VAMPIRE_FANG, PHEROMONE_JAR};

public class Item
{
	#region vars
	public ItemName name;
	public ItemType itemType;
	public float coolDownDelay = 0.0f;  //How long the item takes to cool down
	public float coolDownTimer = 0.0f;  //Current status ( 0.0f = ready )
	#endregion
	
	public void Construct( ItemName itemName )
	{
		//Set up the stats for the item, based on its name.
		name = itemName;
		if ( itemName == ItemName.STOPWATCH )
		{
			itemType = ItemType.ITEM_FAST;
			coolDownDelay = 30.0f;
		}
		else if ( itemName == ItemName.AURA_DEFENSE )
		{
			itemType = ItemType.ITEM_FAST;
			coolDownDelay = 10.0f;
		}
		else if ( itemName == ItemName.AURA_OFFENSE )
		{
			itemType = ItemType.ITEM_FAST;
			coolDownDelay = 10.0f;
		}
		else if ( itemName == ItemName.AURA_REGEN )
		{
			itemType = ItemType.ITEM_FAST;
			coolDownDelay = 10.0f;
		}
		else if ( itemName == ItemName.VAMPIRE_FANG )
		{
			itemType = ItemType.ITEM_CHARGE_AND_RELEASE;
			coolDownDelay = 30.0f;
		}
		else if ( itemName == ItemName.PHEROMONE_JAR )
		{
			itemType = ItemType.ITEM_AIM_POINT;
			coolDownDelay = 30.0f;
		}
	}
	
	// Update is called once per frame (from the player class)
	public void Update ( float dt ) 
	{
		coolDownTimer = Mathf.Max ( coolDownTimer - dt, 0.0f );
	}
}
