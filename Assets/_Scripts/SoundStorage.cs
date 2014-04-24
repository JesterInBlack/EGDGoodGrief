using UnityEngine;
using System.Collections;

public class SoundStorage : MonoBehaviour 
{

	#region vars
	//Set all these in editor.
	public static AudioClip MonkStoneSkinOn;
	public static AudioClip MonkPunch;
	public static AudioClip MonkSandStorm;
	public static AudioClip MonkRockRaise;
	public static AudioClip MonkRockBreak;
	public static AudioClip MonkImpact;

	public static AudioClip KnightBlastOff;
	public static AudioClip KnightRev;
	public static AudioClip KnightRevLoop;
	public static AudioClip KnightSlice;
	public static AudioClip KnightSwoosh;
	public static AudioClip KnightFireSword;

	public static AudioClip ArcherFire;
	public static AudioClip ArcherBlowback;
	public static AudioClip ArcherArrowImpact;

	public static AudioClip BossImpale;
	public static AudioClip BossWeb;
	public static AudioClip BossLaser;
	public static AudioClip BossPointLaser;
	public static AudioClip BossSuction;
	public static AudioClip BossLegHit;
	public static AudioClip BossHit1;
	public static AudioClip BossHit2;
	public static AudioClip BossLegBreak;
	public static AudioClip BossDeathScream;
	public static AudioClip BossStep;
	public static AudioClip BossFall;
	public static AudioClip BossCrunch;
	public static AudioClip BossMegaFlare;
	public static AudioClip BossPowerUp;

	public static AudioClip ItemJarShatter;
	public static AudioClip ItemVampFang;
	public static AudioClip ItemSlowMoIn;
	public static AudioClip ItemSlowMoOut;
	public static AudioClip ItemBuff;
	public static AudioClip ItemDebuff;

	public static AudioClip MenuCancel;
	public static AudioClip MenuBack;
	public static AudioClip MenuConfirm;

	public static AudioClip PlayerHurt1;
	public static AudioClip PlayerHurt2;
	public static AudioClip PlayerBlock1;
	public static AudioClip PlayerBlock2;
	#endregion

	void Awake()
	{
		//Load up all the sounds
		//ugh... these paths should be more modable, and a naming convention would be nice.
		MonkStoneSkinOn = Resources.Load<AudioClip>( "_Sounds/Monk/block" );
		MonkPunch = Resources.Load<AudioClip>( "_Sounds/Monk/punch" );
		MonkSandStorm = Resources.Load<AudioClip>( "_Sounds/Monk/sand_flow" );
		MonkRockRaise = Resources.Load<AudioClip>( "_Sounds/Monk/rockslide" );
		MonkRockBreak = Resources.Load<AudioClip>( "_Sounds/Monk/rocks" );

		KnightBlastOff = Resources.Load<AudioClip>( "_Sounds/RocketSword/launch_proto" );
		KnightRev = Resources.Load<AudioClip>( "_Sounds/RocketSword/single_rev" );
		KnightRevLoop = Resources.Load<AudioClip>( "_Sounds/RocketSword/rev_cont" );
		KnightSlice = Resources.Load<AudioClip>( "_Sounds/RocketSword/sword_slice" );
		KnightSwoosh = Resources.Load<AudioClip>( "_Sounds/RocketSword/sword_swoosh" );
		KnightFireSword = Resources.Load<AudioClip>( "_Sounds/RocketSword/fire_slow" );

		ArcherFire = Resources.Load<AudioClip>( "_Sounds/Archer/bow_fire" );
		ArcherBlowback = Resources.Load<AudioClip>( "_Sounds/Archer/Wind_Gust" );
		ArcherArrowImpact = Resources.Load<AudioClip>( "_Sounds/Archer/arrow_hit2" );

		PlayerHurt1 = Resources.Load<AudioClip>( "_Sounds/player_hit" );
		PlayerHurt2 = Resources.Load<AudioClip>( "_Sounds/player_hit2" );
		PlayerBlock1 = Resources.Load<AudioClip>( "_Sounds/block1" );
		PlayerBlock2 = Resources.Load<AudioClip>( "_Sounds/block2" );

		BossImpale = Resources.Load<AudioClip>( "_Sounds/Boss/Impale" );
		BossWeb = Resources.Load<AudioClip>( "_Sounds/Boss/web2" );
		BossLaser = Resources.Load<AudioClip>( "_Sounds/Boss/laser2" );
		BossPointLaser = Resources.Load<AudioClip>( "_Sounds/Boss/laser3" );
		BossSuction = Resources.Load<AudioClip>( "_Sounds/Boss/suction2" );
		BossLegHit = Resources.Load<AudioClip>( "_Sounds/Boss/leg_impact" );
		BossLegBreak = Resources.Load<AudioClip>( "_Sounds/Boss/leg_break" );
		BossHit1 = Resources.Load<AudioClip>( "_Sounds/stab_squish_heavy" );
		BossHit2 = Resources.Load<AudioClip>( "_Sounds/stab_squish_light" );
		BossDeathScream = Resources.Load<AudioClip>( "_Sounds/Boss/what_have_i_done_final_bellow" );
		BossStep = Resources.Load<AudioClip>( "_Sounds/Boss/step" );
		BossFall = Resources.Load<AudioClip>( "_Sounds/Boss/fall" );
		BossCrunch = Resources.Load<AudioClip>( "_Sounds/Boss/crunch" );
		BossMegaFlare = Resources.Load<AudioClip>("_Sounds/Boss/Explosion_Mark_DiAngelo");
		BossPowerUp = Resources.Load<AudioClip>("_Sounds/Boss/PowerUp");


		ItemJarShatter = Resources.Load<AudioClip>( "_Sounds/Item/jar_shatter" );
		ItemSlowMoIn = Resources.Load<AudioClip>( "_Sounds/Item/slomo_in" );
		ItemSlowMoOut = Resources.Load<AudioClip>( "_Sounds/Item/slomo_out" );
		ItemVampFang = Resources.Load<AudioClip>( "_Sounds/Item/vampire_suck" );
		ItemBuff = Resources.Load<AudioClip>( "_Sounds/Item/powerup" );
		ItemDebuff = Resources.Load<AudioClip>( "_Sounds/Item/debuff" );

		MenuCancel = Resources.Load<AudioClip>( "_Sounds/Menu/menu_cancel" );
		MenuBack = Resources.Load<AudioClip>( "_Sounds/Menu/menu_back" );
		MenuConfirm = Resources.Load<AudioClip>( "_Sounds/Menu/menu_select" );
	}
}
