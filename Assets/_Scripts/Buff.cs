using UnityEngine;
using System.Collections;

public class Buff
{
	#region vars
	public float duration = 0.0f;  //duration of buff
	public float t = 0.0f;         //counts down to 0 from duration
	public bool taggedForRemoval = false; //lifetime has expired flag.
	public bool blacklist = false; //whether this is a blacklist buff or not. (comes off on bothering giver)
	public int giverId = 0;        //the id of the player who gave you the buff.
	public Player player;

	//STAT CHANGES
	public float offense = 0.0f; //added to offense (% damage increase)  (1.0f = + 100% base damage bonus)   (2x = 2x)
	public float defense = 0.0f; //added to defense (% damage reduction) (1.0f =   100% -> 50% damage taken) (2x = 2x)
	//public float speed   = 0.0f; //added to move speed (1.0f = + 1 unit / s)
	public float regen   = 0.0f; //regen / degen (hp / s)
	//public float maxHp;        //added to max hp for duration
	#endregion

	//Use for initialization
	public void Start()
	{
		t = duration;
		player.offense += offense;
		player.defense += defense;
	}

	//Use for cleanup
	public void End()
	{
		player.offense -= offense;
		player.defense -= defense;
	}

	public bool isTheSameAs( Buff buff )
	{
		//checks if two buffs are the same
		//if effect + giver matches
		if ( buff.giverId == giverId && buff.regen == regen && buff.offense == offense && buff.defense == defense )
		{
			return true;
		}
		//omit else
		return false;
	}
	
	// Update is called once per frame
	public void Update ( float dt ) 
	{
		#region timer
		t -= dt;
		if ( t <= 0.0f )
		{
			taggedForRemoval = true;
		}
		#endregion

		//Regeneration
		if ( player.HP > 0.0f ) //Only regenerate / degenerate if you're alive.
		{
			player.HP = Mathf.Max ( 1.0f, player.HP + regen * dt ); //degeneration can't kill you.
			player.HP = Mathf.Min ( player.HP, player.maxHP ); //over cap check
		}
	}
}
