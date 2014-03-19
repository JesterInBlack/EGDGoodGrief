using UnityEngine;
using System.Collections;

public class RocketHUD : MonoBehaviour 
{

	//TODO: make this handle only the rocket sword part of the code, move hp bar into own code.
	#region vars
	public GameObject myPlayer; //set in inspector
	
	public Texture[] hudBackgrounds = new Texture[9]; //set in inspector
	public GUITexture hudBackground; //set in inspector
	//TODO: accent color

	public GameObject HPBarFill;     //HP bar            (set in inspector)
	public GameObject HPBarLerpFill; //HP bar lerp       (set in inspector)
	public GameObject HPBarMaxFill;  //HP bar max        (set in inspector)
	//public GameObject HPBarOverlay;  //Overlay animation (set in inspector)
	//TODO: light at edge of HP

	private float lerpHP; //?
	private float prevLerpHP;
	private float nextLerpHP;
	private float lerpT;
	//private float animationX;

	public Texture2D needleTexture;  //set in inspector
	public Texture2D dialTexture;    //set in inspector

	public enum ScreenCorner { UPPER_RIGHT, UPPER_LEFT, LOWER_RIGHT, LOWER_LEFT };
	public ScreenCorner screenCorner;

	public float dialCoverRotation;  //rotation of dial cover sprite (set in inspector)
	public float dialBaseRotation;   //rotation of dial @ 0    charge (set in inspector)
	public float dialFullRotation;   //rotation of dial @ 100% charge (set in inspector)

	private Player player;
	private int prevStage = 0;
	#endregion

	// Use this for initialization
	void Start () 
	{
		player = myPlayer.GetComponent<Player>();
		if ( player.characterclass != CharacterClasses.KNIGHT ) 
		{ 
			//this.enabled = false; 
			Destroy ( this.gameObject );
		}

		lerpHP = player.baseMaxHP;
		prevLerpHP = player.baseMaxHP;
		nextLerpHP = player.baseMaxHP;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//animationX += Time.deltaTime; //TODO: dt
		//animation.guiTexture
		//animation.guiTexture.border.right = 0;
		//animation.guiTexture.border.left = 0;
		//now we need to make it loop.
		//GODDAMN IT UNITY! Y U NO KNOW HOW TO SHOW IMAGE PARTS!?

		int bgStage = (int)(player.resource * 8.0f);
		if ( bgStage != prevStage ) //stage has changed!
		{
			hudBackground.texture = hudBackgrounds[ bgStage ];
		}
		prevStage = bgStage;

		HPBarStuff();
	}

	void OnGUI ()
	{
		//TODO: lerp this angle a bit more.

		//THIS NEEDS TO BE HARD CODED B/C UNITY DOESN'T KNOW HOW TO ROTATE GUITEXTURES
		//SO WE NEED ACTUAL SCREEN PIXELS INSTEAD OF VIEWPORT % WITH OFFSETS.
		//TERRIBLE, TERRIBLE practice, and a huge mess.
		//UR: 116, 138
		//UL: 
		//LR: 
		//LL: 
		Vector2 pivot = new Vector2( 116.0f, 138.0f );
		if ( screenCorner == ScreenCorner.UPPER_LEFT )
		{
			pivot = new Vector2( 116.0f, 138.0f );
		}
		else if ( screenCorner == ScreenCorner.UPPER_RIGHT )
		{
			pivot = new Vector2( Screen.width - 116.0f, 138.0f );
		}
		else if ( screenCorner == ScreenCorner.LOWER_LEFT )
		{
			pivot = new Vector2( 116.0f, Screen.height - 138.0f );
		}
		else if ( screenCorner == ScreenCorner.LOWER_RIGHT )
		{
			pivot = new Vector2( Screen.width - 116.0f, Screen.height - 138.0f );
		}

		float angle = Mathf.Lerp ( dialBaseRotation, dialFullRotation, player.resource );

		GUIUtility.RotateAroundPivot ( dialCoverRotation, pivot );
		GUI.DrawTexture ( new Rect( pivot.x - 12.0f, pivot.y - 12.0f, 24.0f, 24.0f), dialTexture, ScaleMode.StretchToFill );
		GUIUtility.RotateAroundPivot ( -1.0f * dialCoverRotation, pivot );

		GUIUtility.RotateAroundPivot ( angle, pivot );
		GUI.DrawTexture( new Rect( pivot.x - 6.0f, pivot.y - 53.0f, 12.0f, 106.0f), needleTexture, ScaleMode.StretchToFill );
	}

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
}
