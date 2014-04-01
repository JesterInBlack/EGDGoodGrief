using UnityEngine;
using System.Collections;

public static class ScoreManager
{
	//A class to neatly contain all the score manipulation code.
	//Interfaces with the players array in gamestate.
	public static void Death( int id )
	{
		//Take 25% of thier points.
		GameState.players[id].GetComponent<Player>().score -= GameState.players[id].GetComponent<Player>().score * 0.25f;
	}

	public static void DealtDamage( int id, float damage )
	{
		//give them points ~ damage
		GameState.players[id].GetComponent<Player>().score += damage;
	}

	public static void TookDamage( int id, float damage )
	{
		//take away points (~ damage?)
		GameState.players[id].GetComponent<Player>().score -= damage;
	}

	public static void AvoidedDamage( int id, float damage )
	{
		//give them points ~ damage
		GameState.players[id].GetComponent<Player>().score += damage / 2.0f;
	}

	//BONUS OBJECTIVES
	public static void LastHit( int id )
	{
		//bonus points for last hitting boss.
		GameState.players[id].GetComponent<Player>().score += 1000.0f;
	}

	public static void KilledLeg( int id )
	{
		//bonus points for last hitting leg.
		GameState.players[id].GetComponent<Player>().score += 25.0f;
	}

	public static void BrokeArmor( int id )
	{
		//bonus points for last hitting leg armor.
		GameState.players[id].GetComponent<Player>().score += 5.0f;
	}

	//TODO: helped revive?
	//TODO: killing adds
	//TODO: unity participation / helping bonus
	//TODO: dissension bonus for not getting screwed by the dissension attack.

	//TODO: "GLORIOUS" indicator.
}
