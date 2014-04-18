using UnityEngine;
using System.Collections;

public class ItemHandler : MonoBehaviour 
{
	//A class to handle item usage.

	#region vars
	public GameObject jarPrefab; //set in inspector.

	private Player player;
	#endregion

	// Use this for pre-initialization
	void Awake ()
	{
		player = GetComponent<Player>();
	}

	//-------------------------------------------------------------------------------------------------------
	//ITEMS?
	//-------------------------------------------------------------------------------------------------------
	public void ItemButtonPressed()
	{
		//called on use item button pressed.
		if ( player.isDowned ) { return; } //Can't use items while downed.
		if ( player.items[ player.itemIndex ].coolDownTimer <= 0.0f )
		{
			GetComponent<Tutorial>().usedItem = true;
			if ( player.items[ player.itemIndex ].itemType == ItemType.ITEM_FAST )
			{
				//Do the effect. NOW!
				player.state = "item windup";
				player.stateTimer = 0.05f * 12.0f; //frames
				player.nextState = "idle";
				UseItem ( player.itemIndex );
			}
			else if ( player.items[ player.itemIndex ].itemType == ItemType.ITEM_CHARGE_AND_RELEASE )
			{
				//Set up holding state, do the effect on release.
				player.state = "item windup";
				player.stateTimer = 0.05f * 2.0f; //frames
				player.nextState = "item charge";
			}
			else if ( player.items[ player.itemIndex ].itemType == ItemType.ITEM_AIM_RAY)
			{
				//Set up aiming state, do the effect on release.
				player.state = "item windup";
				player.stateTimer = 0.05f * 2.0f; //frames
				player.nextState = "item aim ray";
			}
			else if ( player.items[ player.itemIndex ].itemType == ItemType.ITEM_AIM_POINT )
			{
				//Set up aiming state, do the effect on release.
				player.state = "item windup";
				player.stateTimer = 0.05f * 2.0f; //frames
				player.nextState = "item aim point";
			}
		}
		else
		{
			//Still on cooldown.
		}
	}
	
	public void ItemButtonReleased()
	{
		//called on use item button released.
		if ( player.isDowned ) { return; } //Can't use items while downed.
		if ( player.items[ player.itemIndex ].coolDownTimer <= 0.0f )
		{
			UseItem ( player.itemIndex );
		}
	}
	
	public void UseItem( int index )
	{
		//This function is called when an item's effect is to take place.
		//IE: this comes after the charging / aiming junk
		//it does the effect + sets the cooldown
		
		player.items[ index ].coolDownTimer = player.items[ index ].coolDownDelay; //set cooldown
		#region effects
		//DO EFFECT:
		if ( player.items[index].name == ItemName.STOPWATCH )
		{
			StopWatch ();
		}
		else if ( player.items[index].name == ItemName.AURA_DEFENSE )
		{
			const float duration = 60.0f;
			Aura ( player.id, duration, 0.0f, 1.0f, 0.0f );
		}
		else if ( player.items[index].name == ItemName.AURA_OFFENSE )
		{
			const float duration = 60.0f;
			Aura ( player.id, duration, 1.0f, 0.0f, 0.0f );
		}
		else if ( player.items[index].name == ItemName.AURA_REGEN )
		{
			const float duration = 60.0f;
			Aura ( player.id, duration, 0.0f, 0.0f, 1.0f );
		}
		else if ( player.items[index].name == ItemName.VAMPIRE_FANG )
		{
			//TODO: charge
			//TODO: hit detection box at position after animating:
			//on success: trigger suck life
			float angle = GetComponent<CustomController>().facing * Mathf.PI / 2.0f;
			float x = transform.position.x;
			float y = transform.position.y;
			float min = 0.5f; //minimum or base width & height of the hitbox
			float r = 1.0f;   //factor applied to cos / sin to extend hitbox
			//
			float xmin = Mathf.Min ( x - min, x - min + r * Mathf.Cos ( angle ) );
			float ymin = Mathf.Min ( y - min, y - min + r * Mathf.Sin ( angle ) );
			float xmax = Mathf.Max ( x + min, x + min + r * Mathf.Cos ( angle ) );
			float ymax = Mathf.Max ( y + min, y + min + r * Mathf.Sin ( angle ) );
			if ( AttackSystem.getHitsInBox( new Rect( xmin, ymin, (xmax - xmin), (ymax - ymin) ), player.id ).Length > 0 )
			{
				player.state = "vampire";
				player.stateTimer = 1.5f;
				player.nextState = "idle";
				player.canMove = false;
			}
		}
		else if ( player.items[index].name == ItemName.PHEROMONE_JAR )
		{
			//TODO: animate
			GameObject jar = (GameObject)Instantiate ( jarPrefab, transform.position, Quaternion.identity );
			jar = jar.GetComponent<JarLink>().jar;
			jar.GetComponent<LobbedProjectile>().id = player.id;
			jar.GetComponent<LobbedProjectile>().aimPoint = GetComponent<CustomController>().aimPoint;
			jar.GetComponent<LobbedProjectile>().Fire ( 
			                                           new Vector2( transform.position.x, transform.position.y ), 
			                                           new Vector2( GetComponent<CustomController>().aimPoint.x, GetComponent<CustomController>().aimPoint.y ), 
			                                           11.0f );
			//State stuff
			player.state = "item windown"; //item wind down?
			player.stateTimer = 0.05f * 12.0f; //frames for animation.
			player.nextState = "idle";
		}
		#endregion
		#region animation
		gameObject.GetComponent<Animator>().Play( "throw_" +  player.GetAniSuffix() );
		#endregion
	}
	
	private void StopWatch()
	{
		player.isInBulletTime = true;
		player.bulletTimeDuration = 6.0f;
		StaticData.t_scale = 0.5f;
		StaticData.bulletTimeDuration = player.bulletTimeDuration;
		//TODO: lerp in, add visual effect, play sound
		//TODO: duration on t scale, lerp back in, remove visual effect, play sound
	}
	
	private void Aura( int id, float duration, float offense, float defense, float regen )
	{
		//Give all players the blacklist buff.
		//TODO: sound?
		//TODO: don't allow players to stack the same buff from the same source multiple times on themselves.
		//      instead, refresh it.
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			Player targetPlayer = GameState.players[i].GetComponent<Player>();
			Buff myBuff = new Buff();
			myBuff.player = targetPlayer;
			myBuff.giverId = id;
			myBuff.blacklist = true;
			myBuff.offense = offense;
			myBuff.defense = defense;
			myBuff.regen = regen;
			myBuff.duration = duration;
			//add to player and apply effect
			//FIRST: change stacking from the same source -> refresh 
			//remove any old buffs that match it with the same source
			//(so you can't multi-stack the same aura on yourself from yourself 3 times,
			// 12x of the same aura buffs is a scaling nightmare, and would require nerfing auras to obsolescence)
			//Also, this lets us make the CD shorter than the duration
			for ( int j = targetPlayer.buffs.Count - 1; j >= 0; j-- )
			{
				if ( ( (Buff) targetPlayer.buffs[j] ).isTheSameAs( myBuff ) )
				{
					( (Buff) targetPlayer.buffs[j] ).End();
					//Debug.Log ( "Buff refreshed on player " + (i + 1) );
					targetPlayer.buffs.RemoveAt ( j );
				}
			}
			GameState.players[i].GetComponent<Player>().buffs.Add ( myBuff );
			myBuff.Start ();
			//Debug.Log ( "Buff started on player " + (i + 1) );
		}
	}
}
