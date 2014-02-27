using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour 
{
	//Class to handle boss AI, etc.

	#region vars
	public float hp;
	public float maxhp;

	//complexity: AI, aggro, attack logic, stats by part?
	#endregion

	// Use this for initialization
	void Start () 
	{
		maxhp = 10000.0f;
		hp = maxhp;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Time.deltaTime * StaticData.t_scale;
		GetComponent<GUIText>().text = hp + " / " + maxhp;
	}

	//--------------------------------------------------------------------------------------------------------------------
	public void Hurt( float damage )
	{
		//TODO: make this depend on hit detection, parts of body (core?).
		hp -= damage;
	}

	//--------------------------------------------------------------------------------------------------------------------
	//Attacks
	//--------------------------------------------------------------------------------------------------------------------
	void Attack()
	{
		//prototype attack function
	}

	void Impale()
	{
		//Stab an enemy.
		//Does bonus venom damage if venomous buff is up.
	}

	void Rake()
	{
		//Stab an enemy, and rake claws in a path against the ground back to main body.
		//Does bonus venom damage if venomous buff is up.
	}

	void Bite()
	{
		//Bites an enemy
		//Does bonus venom damage if fangs are venomous
	}

	void Suction()
	{
		//Suction attack: draw players toward mouth. After a while, accelerate the suction until a player is eaten.
		//Then, proceed to chomp them for heals
		//Then, spit them out (along with wave of adds?)
	}

	void SpawnHeadGrabber()
	{
		//Spawns a head grabber add.
		//these move fast and will chase the nearest player, jump on thier head, and after some time explode.
		//they can be moved onto another playre by touching them.
		//This spawn should be rare-ish.
	}

	void BarfSpiders()
	{
		//Spawn tons of mini-adds + add venom to bites
	}

	void ShootWebbing()
	{
		//Lobs webbing at a location, causing it to become sticky and cause slow for a time.
	}

	void ShootVenom()
	{
		//Lobs venom at a location, causing a pool of DoT that fades.
	}

	void VenomFangs()
	{
		//Enchant bite with venom
		//  adds a venom drip effect to fangs, which persists for a long time.
		//  Also, spawns a pool of venom where the spider currently is.
	}

	void VenomBuff()
	{
		//Enchant leg with venom damage
	}

	void ArmorBuff()
	{
		//Enchant leg with armor (add web armor to it)
	}

	void SpawnBuff()
	{
		//Enchant leg with "spawn mini adds on being hit" effect.
	}

	void Laser()
	{
		//Shoot an eye laser at a target.
		//Secondary effect: causes the ground to crack
		//  the crack then begins to glow red
		//  lava explodes out of the fissure for damage
		//Telegraph with charge
	}

	void Beam()
	{
		//Destruction beam
		//  Begins as a thin, sustained laser.
		//  Eye tracks slowly during this attack.
		//  Laser gradually becomes wider + more damaging.
		//  If the players cause the spider to spin around enough during the attack, 
		//    the spider becomes dizzy and falls over.
		//Telegraph with charge
	}

	void Rappel()
	{
		//The spider jumps up into the "sky".
	}

	void Chandelier()
	{
		//Spider rakes across the screen, doing aoe damage in a huge area.
		//Swings in a parabolic arc? Does linear damage
		//Telegraph with shadow
		//Can only be done after rappelling
	}

	void BombingRun()
	{
		//Drop eggs while swinging across the scene.
		//Some explode, some spawn adds
		//Can only be done after rappelling
	}

	void JumpDown()
	{
		//Can only be done after rappelling
		//+effect: causes stalagtites to fall
	}

	void Collapse()
	{
		//Spider crashes into the ground, <becoming attackable?>
		//Does damage to players under + around it
		//+effect: causes stalagtites to fall
	}

	void LayEgg()
	{
		//Spawns an egg that hatches into a spider.
		//  Exploding egg
		//  Kamikaze
		//  Head Grabber
		//  Jar spiders (healing drones)
		//  Queen (spawns mini adds)
		//  Normal boring adds
		//  Mini adds (swarmers)
	}
}
