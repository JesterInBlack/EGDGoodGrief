using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
	//TODO: rename this
	//TODO: animate HP bars
	#region vars
	public GameObject myPlayer; //set in inspector
	//TODO: add image list for items
	
	public Texture2D HPBarFill;     //HP bar       (set in inspector)
	public Texture2D HPBarLerpFill; //HP bar lerp  (set in inspector)
	public Texture2D HPBarMaxFill;  //HP bar max   (set in inspector)
	public Texture2D HPBarOverlay;  //Overlay      (set in inspector)
	public Texture2D HPBarAni;      //Animation    (set in inspector)
	public Texture2D HPBarLight;    //Edge light   (set in inspector)
	public Texture2D itemSelector;  //Selector     (set in inspector)
	public Texture2D aButton;       //A button     (set in inspector)
	public Texture2D aButtonGray;   //A button     (set in inspector)
	//Custom parts
	public Texture2D knightBG;      //Background   (set in inspector)
	public Texture2D archerBG;      //Background   (set in inspector)
	public Texture2D monkBG;        //Background   (set in inspector)
	public Texture2D ninjaBG;       //Background   (set in inspector)
	public Texture2D needleTexture; //             (set in inspector)
	public Texture2D sandTexture;   //             (set in inspector)
	public Texture2D focusTexture;  //             (set in inspector)
	public Texture2D styleTexture;  //             (set in inspector)

	public enum ScreenCorner { UPPER_RIGHT, UPPER_LEFT, LOWER_RIGHT, LOWER_LEFT };
	public ScreenCorner screenCorner;
	public Vector2 HPBarOffset;      //HP bar's offset.               (set in inspector)
	public Vector2 itemPos1;         //item offset                    (set in inspector)
	public Vector2 itemPos2;         //item offset                    (set in inspector)
	public Vector2 itemPos3;         //item offset                    (set in inspector)
	private bool flipAnchor = false; //do we flip the way HP fills?
	private bool upsideDown = false; //is it upside down?
	//Rocket Sword
	public Vector2 pivot;            //pivot point for the needle     (set in inspector)
	public float dialBaseRotation;   //rotation of dial @ 0    charge (set in inspector)
	public float dialFullRotation;   //rotation of dial @ 100% charge (set in inspector)
	//Monk
	public Vector2 sandOffset;
	//Archer
	//public Vector2 focusOffset;
	//Ninja
	//public Vector2 styleOffset;
	
	private Vector2 size =  new Vector2( 225.0f, 125.0f ); //the size of all the HUD backgrounds.

	private float lerpHP; //?
	private float prevLerpHP;
	private float nextLerpHP;
	private float lerpT;
	private float aniT; //animation time

	private Player player;
	#endregion
	
	// Use this for initialization
	void Start () 
	{
		player = myPlayer.GetComponent<Player>();

		if ( screenCorner == ScreenCorner.LOWER_RIGHT || screenCorner == ScreenCorner.UPPER_RIGHT )
		{
			flipAnchor = true;
		}
		if ( screenCorner == ScreenCorner.LOWER_LEFT || screenCorner == ScreenCorner.LOWER_RIGHT )
		{
			upsideDown = true;
		}
		
		lerpHP = player.baseMaxHP;
		prevLerpHP = player.baseMaxHP;
		nextLerpHP = player.baseMaxHP;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//HPBarStuff();
	}
	
	void OnGUI ()
	{
		//TODO: lerp this angle a bit more.

		//Vector2 size =  new Vector2( 225.0f, 125.0f ); //the size of all the HUD backgrounds.
		Vector2 pos = new Vector2( 0.0f, 0.0f );       //the position of the HUD background's upper-left corner
		if ( screenCorner == ScreenCorner.UPPER_LEFT )
		{
			pos = new Vector2( 0.0f, 0.0f );
		}
		else if ( screenCorner == ScreenCorner.UPPER_RIGHT )
		{
			pos = new Vector2( Screen.width - size.x, 0.0f );
		}
		else if ( screenCorner == ScreenCorner.LOWER_LEFT )
		{
			pos = new Vector2( 0.0f, Screen.height - size.y );
		}
		else if ( screenCorner == ScreenCorner.LOWER_RIGHT )
		{
			pos = new Vector2( Screen.width - size.x, Screen.height - size.y );
		}

		//Draw HP bar 1st : for all classes
		HPBarStuff( pos );

		//Now we draw non-common elements
		if ( player.characterclass == CharacterClasses.KNIGHT ) 
		{
			KnightGUI( pos );
		}
		else if ( player.characterclass == CharacterClasses.ARCHER )
		{
			ArcherGUI( pos );
		}
		else if ( player.characterclass == CharacterClasses.DEFENDER )
		{
			MonkGUI( pos );
		}
		else if ( player.characterclass == CharacterClasses.NINJA )
		{
			NinjaGUI( pos );
		}

		//Draw Items on top.
		ItemStuff( pos );

		//Draw HP threshold for revive.
		DrawReviveThreshold( pos );
	}

	void HPBarStuff( Vector2 pos )
	{
		//! Handles drawing HP bars for all HUDs
		/*! \param pos top left screen coordinate of the GUI*/
		float x, y, width, height;
		float percentHP = player.HP / player.baseMaxHP;
		float percentMaxHP = player.maxHP / player.baseMaxHP;

		//if anchor's flipped, then anchor to the right.

		//Max
		x = pos.x + HPBarOffset.x;
		y = pos.y + HPBarOffset.y;
		width = Mathf.Max ( 0.0f, percentMaxHP * HPBarMaxFill.width );
		height = HPBarMaxFill.height;
		if ( flipAnchor )
		{
			x = x + (HPBarMaxFill.width - width);
		}
		GUI.DrawTexture ( new Rect( x, y, width, height ), HPBarMaxFill );

		//Lerp

		//Fill
		x = pos.x + HPBarOffset.x;
		y = pos.y + HPBarOffset.y;
		width = Mathf.Max ( 0.0f, percentHP * HPBarFill.width );
		height = HPBarFill.height;
		if ( flipAnchor )
		{
			x = x + (HPBarFill.width - width);
		}
		if ( ! player.isDowned )
		{
			GUI.DrawTexture ( new Rect( x, y, width, height ), HPBarFill );
		}
		else
		{
			//downed: change color to red
			GUI.DrawTexture ( new Rect( x, y, width, height ), HPBarLerpFill );
		}

		//Animated Overlay
		aniT += Time.deltaTime * 8.0f;
		if ( aniT > HPBarFill.width ) 
		{
			aniT -= HPBarFill.width;
		}

		x = pos.x + HPBarOffset.x;
		y = pos.y + HPBarOffset.y;
		width = Mathf.Max ( 0.0f, percentHP * HPBarFill.width );
		height = HPBarFill.height;
		if ( flipAnchor )
		{
			x = x + (HPBarFill.width - width);
		}
		GUI.BeginGroup ( new Rect( x, y, width, height ) );
		GUI.DrawTexture ( new Rect( -HPBarFill.width + aniT, 0.0f, HPBarAni.width, HPBarAni.height ), HPBarAni, ScaleMode.StretchToFill, true );
		GUI.DrawTexture ( new Rect( aniT, 0.0f, HPBarAni.width, HPBarAni.height ), HPBarAni, ScaleMode.StretchToFill, true );
		GUI.EndGroup ();

		//Overlay
		x = pos.x + HPBarOffset.x;
		y = pos.y + HPBarOffset.y;
		width = HPBarOverlay.width;
		height = HPBarOverlay.height;
		GUI.DrawTexture ( new Rect(x, y, width, height), HPBarOverlay );
	}

	/*
	void HPBarStuff()
	{
		#region HP Bar
		float x, y, width, height;
		
		//Current
		//float percentHP = myPlayerState.HP / myPlayerState.baseMaxHP;
		float percentMaxHP = player.maxHP / player.baseMaxHP;
		float percentLerpHP = lerpHP / player.baseMaxHP;
		float percentPrevHP = prevLerpHP / player.baseMaxHP;
		
		//LERP
		//lerp any damage? (dark red bg, green -> red smoothly goes down?)
		GUITexture tempHPBarFill = HPBarFill.GetComponent<GUITexture>();
		x = (int) tempHPBarFill.pixelInset.x;
		y = (int) tempHPBarFill.pixelInset.y;
		height = (int) tempHPBarFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 180.0f * percentLerpHP );
		tempHPBarFill.pixelInset = new Rect(x, y, width, height);
		
		if ( lerpHP != nextLerpHP )
		{
			lerpT = Mathf.Min ( lerpT + Time.deltaTime * 5.0f, 1.0f );
			lerpHP = Mathf.Lerp ( prevLerpHP, nextLerpHP, lerpT );
		}
		if ( lerpHP == nextLerpHP || lerpT == 1.0f )
		{
			prevLerpHP = nextLerpHP;
			nextLerpHP = player.HP;
			lerpT = 0.0f;
		}
		
		//Previous hp
		GUITexture tempHPBarLerpFill = HPBarLerpFill.GetComponent<GUITexture>();
		x = (int) tempHPBarLerpFill.pixelInset.x;
		y = (int) tempHPBarLerpFill.pixelInset.y;
		height = (int) tempHPBarLerpFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 180.0f * percentPrevHP );
		tempHPBarLerpFill.pixelInset = new Rect(x, y, width, height);
		
		//fade out
		tempHPBarLerpFill.color = new Color( 1.0f, 1.0f, 1.0f, Mathf.Max ( 0.0f, Mathf.Min( 1.0f, (1.0f - lerpT) * 2.0f ) ) );
		
		//Max
		GUITexture tempHPBarMaxFill = HPBarMaxFill.GetComponent<GUITexture>();
		x = (int) tempHPBarMaxFill.pixelInset.x;
		y = (int) tempHPBarMaxFill.pixelInset.y;
		height = (int) tempHPBarMaxFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 180.0f * percentMaxHP );
		tempHPBarMaxFill.pixelInset = new Rect(x, y, width, height);
		#endregion
	}
	*/

	void ItemStuff( Vector2 pos )
	{
		//! Handles drawing items for all HUDs
		/*! \param pos top left screen coordinate of the GUI*/

		//gray them out proportionally to their CD
		//100% -> 100%
		//99% -> 75%
		//0% -> 25%
		//LT
		float f = 1.0f - (player.items[0].coolDownTimer / player.items[0].coolDownDelay);
		if ( f < 1.0f ) 
		{ 
			f = f * 0.65f + 0.25f; 
		}
		GUI.color = new Color( f, f, f, f );
		GUI.DrawTexture ( new Rect( pos.x + itemPos1.x, pos.y + itemPos1.y, 32, 32 ), ItemImages.getImage ( player.items[0].name ) );

		//Bumper?
		f = 1.0f - (player.items[1].coolDownTimer / player.items[1].coolDownDelay);
		if ( f < 1.0f ) 
		{ 
			f = f * 0.65f + 0.25f; 
		}
		GUI.color = new Color( f, f, f, f );
		GUI.DrawTexture ( new Rect( pos.x + itemPos2.x, pos.y + itemPos2.y, 32, 32 ), ItemImages.getImage ( player.items[1].name ) );

		//RT
		f = 1.0f - (player.items[2].coolDownTimer / player.items[2].coolDownDelay);
		if ( f < 1.0f ) 
		{ 
			f = f * 0.65f + 0.25f; 
		}
		GUI.color = new Color( f, f, f, f );
		GUI.DrawTexture ( new Rect( pos.x + itemPos3.x, pos.y + itemPos3.y, 32, 32 ), ItemImages.getImage ( player.items[2].name ) );

		//reset color
		GUI.color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );

		//Draw item selector.
		Vector2 selectorOffset = itemPos1;
		if ( player.itemIndex == 1 ) { selectorOffset = itemPos2; }
		else if ( player.itemIndex == 2 ) { selectorOffset = itemPos3; }
		
		GUI.DrawTexture ( new Rect( pos.x + selectorOffset.x, pos.y + selectorOffset.y, 32, 32 ), itemSelector );
	}

	void DrawReviveThreshold( Vector2 pos )
	{
		//! Display revive threshold marker
		/*! \param pos top left screen coordinate of the GUI*/
		if ( ! player.isDowned ) { return; }
		float x = pos.x + HPBarOffset.x + StaticData.percentHPNeededToRevive * HPBarFill.width - aButton.width / 2.0f;
		float y = pos.y + HPBarOffset.y + 2.0f; //TODO: center this using proper width of two images math.
		if ( player.HP >= StaticData.percentHPNeededToRevive * player.baseMaxHP )
		{
			GUI.DrawTexture( new Rect( x, y, aButton.width, aButton.height), aButton );
		}
		else
		{
			GUI.DrawTexture( new Rect( x, y, aButtonGray.width, aButtonGray.height), aButtonGray );
		}
	}

	void KnightGUI( Vector2 pos )
	{
		//! Draws the Rocketsword Knight's HUD. (needle + gauge)
		/*! \param pos top left screen coordinate of the GUI*/

		//Draw background
		GUI.DrawTexture ( new Rect( pos.x, pos.y, size.x, size.y ), knightBG );
		
		//Draw needle
		float angle = Mathf.Lerp( dialBaseRotation, dialFullRotation, player.resource );
		GUIUtility.RotateAroundPivot ( angle, pos + pivot );
		GUI.DrawTexture ( new Rect( pos.x + pivot.x - needleTexture.width / 2.0f, 
		                           pos.y + pivot.y - needleTexture.height / 2.0f, 
		                           needleTexture.width, 
		                           needleTexture.height ), 
		                 needleTexture );
		GUIUtility.RotateAroundPivot ( -1.0f * angle, pos + pivot );
	}

	void ArcherGUI( Vector2 pos )
	{
		//! Draws the Gale Bowlista Archer's HUD.
		/*! \param pos top left screen coordinate of the GUI*/
	}

	void MonkGUI( Vector2 pos )
	{
		//! Draws the Stone Fist Monk's HUD. (sand bar)
		/*! \param pos top left screen coordinate of the GUI*/

		//Draw sand
		float width = sandTexture.width;
		float height = Mathf.Max ( 0.0f, sandTexture.height * player.resource );
		//Use group to cull parts of bars.
		float x = pos.x + sandOffset.x;
		float y = pos.y + sandOffset.y;
		float bary = 0.0f;
		if ( upsideDown ) 
		{ 
			y = y + sandTexture.height * (1.0f - player.resource); 
			bary  = sandTexture.height * (player.resource - 1.0f );
		}

		GUI.BeginGroup ( new Rect( x, y, width, height ) );
		GUI.DrawTexture ( new Rect( 0.0f, bary, width, sandTexture.height ), sandTexture ); //note: relative positioning
		GUI.EndGroup ();

		//Draw background
		GUI.DrawTexture ( new Rect( pos.x, pos.y, size.x, size.y ), monkBG );
	}

	void NinjaGUI( Vector2 pos )
	{
		//! Draws the Ninja's HUD.
		/*! \param pos top left screen coordinate of the GUI*/
	}
}

