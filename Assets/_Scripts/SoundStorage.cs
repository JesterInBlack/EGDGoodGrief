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

		BossImpale = Resources.Load<AudioClip>( "_Sounds/Boss/Impale" );
	}
}
