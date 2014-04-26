using UnityEngine;
using System.Collections;

public class SoundStorage : MonoBehaviour 
{

	#region vars
	//Set all these in editor.
	public static AudioClip MonkStoneSkinOn;
	public static AudioClip MonkPunch;
	public static AudioClip MonkPunch2;
	public static AudioClip MonkPunch3;
	public static AudioClip MonkPunch4;
	public static AudioClip MonkSandStorm;
	public static AudioClip MonkRockRaise;
	public static AudioClip MonkRockBreak;

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
	public static AudioClip BossIndicator;

	public static AudioClip ItemJarShatter;
	public static AudioClip ItemVampFang;
	public static AudioClip ItemSlowMoIn;
	public static AudioClip ItemSlowMoOut;
	public static AudioClip ItemBuff;
	public static AudioClip ItemDebuff;

	public static AudioClip MenuCancel;
	public static AudioClip MenuBack;
	public static AudioClip MenuConfirm;
	public static AudioClip MenuApplause;

	public static AudioClip PlayerHurt1;
	public static AudioClip PlayerHurt2;
	public static AudioClip PlayerHurt3;
	public static AudioClip PlayerBlock1;
	public static AudioClip PlayerBlock2;
	public static AudioClip PlayerCharge;
	#endregion

	void Awake()
	{
		//Load up all the sounds
		//ugh... these paths should be more modable, and a naming convention would be nice.
		LoadAudio ( ref MonkStoneSkinOn, "_Sounds/Monk/block" );
		LoadAudio ( ref MonkPunch, "_Sounds/Monk/punch" );
		LoadAudio ( ref MonkPunch2, "_Sounds/Monk/impact" );
		LoadAudio ( ref MonkPunch3, "_Sounds/Monk/RightCross" );
		LoadAudio ( ref MonkPunch4, "_Sounds/Monk/Jab-SoundBible.com-1806727891" );
		LoadAudio ( ref MonkSandStorm, "_Sounds/Monk/sand_flow" );
		LoadAudio ( ref MonkRockRaise, "_Sounds/Monk/rockslide" );
		LoadAudio ( ref MonkRockBreak, "_Sounds/Monk/rocks" );

		LoadAudio ( ref KnightBlastOff, "_Sounds/RocketSword/launch_proto" );
		LoadAudio ( ref KnightRev, "_Sounds/RocketSword/single_rev" );
		LoadAudio ( ref KnightRevLoop, "_Sounds/RocketSword/rev_cont" );
		LoadAudio ( ref KnightSlice, "_Sounds/RocketSword/sword_slice" );
		LoadAudio ( ref KnightSwoosh, "_Sounds/RocketSword/sword_swoosh" );
		LoadAudio ( ref KnightFireSword, "_Sounds/RocketSword/fire_slow" );

		LoadAudio ( ref ArcherFire, "_Sounds/Archer/bow_fire" );
		LoadAudio ( ref ArcherBlowback, "_Sounds/Archer/Wind_Gust" );
		LoadAudio ( ref ArcherArrowImpact, "_Sounds/Archer/arrow_hit2" );

		LoadAudio ( ref PlayerHurt1, "_Sounds/player_hit" );
		LoadAudio ( ref PlayerHurt2, "_Sounds/player_hit2" );
		LoadAudio ( ref PlayerHurt3, "_Sounds/player_hit3" );
		LoadAudio ( ref PlayerBlock1, "_Sounds/block1" );
		LoadAudio ( ref PlayerBlock2, "_Sounds/block2" );
		LoadAudio ( ref PlayerCharge, "_Sounds/charge2" );

		LoadAudio ( ref BossImpale, "_Sounds/Boss/Impale" );
		LoadAudio ( ref BossWeb, "_Sounds/Boss/web2" );
		LoadAudio ( ref BossLaser, "_Sounds/Boss/laser2_v2" );
		LoadAudio ( ref BossPointLaser, "_Sounds/Boss/laser3" );
		LoadAudio ( ref BossSuction, "_Sounds/Boss/suction2" );
		LoadAudio ( ref BossLegHit, "_Sounds/Boss/leg_impact" );
		LoadAudio ( ref BossLegBreak, "_Sounds/Boss/leg_break" );
		LoadAudio ( ref BossHit1, "_Sounds/stab_squish_heavy" );
		LoadAudio ( ref BossHit2, "_Sounds/stab_squish_light" );
		LoadAudio ( ref BossDeathScream, "_Sounds/Boss/what_have_i_done_final_bellow" );
		LoadAudio ( ref BossStep, "_Sounds/Boss/step" );
		LoadAudio ( ref BossFall, "_Sounds/Boss/fall" );
		LoadAudio ( ref BossCrunch, "_Sounds/Boss/crunch" );
		LoadAudio ( ref BossMegaFlare, "_Sounds/Boss/Explosion_Mark_DiAngelo");
		LoadAudio ( ref BossPowerUp, "_Sounds/Boss/PowerUp");
		LoadAudio ( ref BossIndicator, "_Sounds/Boss/indicator");


		LoadAudio ( ref ItemJarShatter, "_Sounds/Item/jar_shatter" );
		LoadAudio ( ref ItemSlowMoIn, "_Sounds/Item/slomo_in" );
		LoadAudio ( ref ItemSlowMoOut, "_Sounds/Item/slomo_out" );
		LoadAudio ( ref ItemVampFang, "_Sounds/Item/vampire_suck" );
		LoadAudio ( ref ItemBuff, "_Sounds/Item/powerup" );
		LoadAudio ( ref ItemDebuff, "_Sounds/Item/debuff" );

		LoadAudio ( ref MenuCancel, "_Sounds/Menu/menu_cancel" );
		LoadAudio ( ref MenuBack, "_Sounds/Menu/menu_back" );
		LoadAudio ( ref MenuConfirm, "_Sounds/Menu/menu_select" );
		LoadAudio ( ref MenuApplause, "_Sounds/Menu/Applause_Mike_Koenig" );
	}

	private void LoadAudio( ref AudioClip clip, string path )
	{
		//function loads the file at path into the clip variable.
		//Debug.Log ( clip != null );
		if ( clip != null ) { return; } //already loaded, so don't load it again.
		clip = Resources.Load<AudioClip>( path );
	}
}
