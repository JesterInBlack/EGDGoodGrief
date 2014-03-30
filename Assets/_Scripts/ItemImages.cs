using UnityEngine;
using System.Collections;

public static class ItemImages
{
	#region vars
	public static Texture2D phermoneJar;
	public static Texture2D stopWatch;
	public static Texture2D auraOffense;
	public static Texture2D auraDefense;
	public static Texture2D auraHeal;
	public static Texture2D vampireFang;
	//public Texture2D humanShield;

	public static Sprite phermoneJarSprite;
	public static Sprite stopWatchSprite;
	public static Sprite auraOffenseSprite;
	public static Sprite auraDefenseSprite;
	public static Sprite auraHealSprite;
	public static Sprite vampireFangSprite;
	//public static Sprite humanShieldSprite;
	#endregion

	public static void Start() //initialization proxy. (note this is not a monobehaviour)
	{
		//phermoneJar = Resources.Load<Texture2D>( "_Art/GUI/Common/Items/PheromoneJar" );
		phermoneJar = Resources.Load<Texture2D>( "PheremoneJar" );
		stopWatch   = Resources.Load<Texture2D>( "Stopwatch" );
		auraOffense = Resources.Load<Texture2D>( "Offensive Aura" );
		auraDefense = Resources.Load<Texture2D>( "DefensiveAura" );
		auraHeal    = Resources.Load<Texture2D>( "HealingAura" );
		vampireFang = Resources.Load<Texture2D>( "VampFang" );

		phermoneJarSprite = Resources.Load<Sprite>( "PheremoneJar" );
		stopWatchSprite   = Resources.Load<Sprite>( "Stopwatch" );
		auraOffenseSprite = Resources.Load<Sprite>( "Offensive Aura" );
		auraDefenseSprite = Resources.Load<Sprite>( "DefensiveAura" );
		auraHealSprite    = Resources.Load<Sprite>( "HealingAura" );
		vampireFangSprite = Resources.Load<Sprite>( "VampFang" );
	}

	public static Texture2D getImage( ItemName itemName )
	{
		//gets the Texture2D (GUI) version of it
		if ( itemName == ItemName.AURA_DEFENSE )
		{
			return auraDefense;
		}
		else if ( itemName == ItemName.AURA_OFFENSE )
		{
			return auraOffense;
		}
		else if ( itemName == ItemName.AURA_REGEN )
		{
			return auraHeal;
		}
		else if ( itemName == ItemName.PHEROMONE_JAR )
		{
			return phermoneJar;
		}
		else if ( itemName == ItemName.STOPWATCH )
		{
			return stopWatch;
		}
		else if ( itemName == ItemName.VAMPIRE_FANG )
		{
			return vampireFang;
		}
		//default:
		return auraDefense;
	}

	public static Sprite getSprite( ItemName itemName )
	{
		if ( itemName == ItemName.AURA_DEFENSE )
		{
			return auraDefenseSprite;
		}
		else if ( itemName == ItemName.AURA_OFFENSE )
		{
			return auraOffenseSprite;
		}
		else if ( itemName == ItemName.AURA_REGEN )
		{
			return auraHealSprite;
		}
		else if ( itemName == ItemName.PHEROMONE_JAR )
		{
			return phermoneJarSprite;
		}
		else if ( itemName == ItemName.STOPWATCH )
		{
			return stopWatchSprite;
		}
		else if ( itemName == ItemName.VAMPIRE_FANG )
		{
			return vampireFangSprite;
		}
		//default:
		return auraDefenseSprite;
	}
}
