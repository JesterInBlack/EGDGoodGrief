using UnityEngine;
using System.Collections;

public class HUDScript : MonoBehaviour 
{
	#region vars
	public GameObject myPlayer;
	private Player myPlayerState;

	//GUI Objects
	public GameObject HPBarFill;     //HP bar
	public GameObject HPBarLerpFill; //HP bar lerp
	public GameObject HPBarMaxFill;  //HP bar max
	public GameObject ResBarFill;    //Resource bar
	private float lerpHP; //?
	private float prevLerpHP;
	private float nextLerpHP;
	private float lerpT;
	#endregion

	// Use this for initialization
	void Start () 
	{
		myPlayerState = myPlayer.GetComponent<Player>();
		lerpHP = myPlayerState.baseMaxHP;
		prevLerpHP = myPlayerState.baseMaxHP;
		nextLerpHP = myPlayerState.baseMaxHP;
	}
	
	// Update is called once per frame
	void Update () 
	{
		float x, y, width, height;

		#region HP Bar
		//Current
		//float percentHP = myPlayerState.HP / myPlayerState.baseMaxHP;
		float percentMaxHP = myPlayerState.maxHP / myPlayerState.baseMaxHP;
		float percentLerpHP = lerpHP / myPlayerState.baseMaxHP;
		float percentPrevHP = prevLerpHP / myPlayerState.baseMaxHP;

		//LERP
		//lerp any damage? (dark red bg, green -> red smoothly goes down?)
		GUITexture tempHPBarFill = HPBarFill.GetComponent<GUITexture>();
		x = (int) tempHPBarFill.pixelInset.x;
		y = (int) tempHPBarFill.pixelInset.y;
		height = (int) tempHPBarFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 100.0f * percentLerpHP );
		tempHPBarFill.pixelInset = new Rect(x, y, width, height);
		
		if ( lerpHP != nextLerpHP )
		{
			lerpT = Mathf.Min ( lerpT + Time.deltaTime * 5.0f, 1.0f );
			lerpHP = Mathf.Lerp ( prevLerpHP, nextLerpHP, lerpT );
		}
		if ( lerpHP == nextLerpHP || lerpT == 1.0f )
		{
			prevLerpHP = nextLerpHP;
			nextLerpHP = myPlayerState.HP;
			lerpT = 0.0f;
		}

		//Previous hp
		GUITexture tempHPBarLerpFill = HPBarLerpFill.GetComponent<GUITexture>();
		x = (int) tempHPBarLerpFill.pixelInset.x;
		y = (int) tempHPBarLerpFill.pixelInset.y;
		height = (int) tempHPBarLerpFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 100.0f * percentPrevHP );
		tempHPBarLerpFill.pixelInset = new Rect(x, y, width, height);

		//fade out
		tempHPBarLerpFill.color = new Color( 1.0f, 1.0f, 1.0f, Mathf.Max ( 0.0f, Mathf.Min( 1.0f, (1.0f - lerpT) * 2.0f ) ) );

		//Max
		GUITexture tempHPBarMaxFill = HPBarMaxFill.GetComponent<GUITexture>();
		x = (int) tempHPBarMaxFill.pixelInset.x;
		y = (int) tempHPBarMaxFill.pixelInset.y;
		height = (int) tempHPBarMaxFill.pixelInset.height;
		width = (int) Mathf.Max ( 0.0f, 100.0f * percentMaxHP );
		tempHPBarMaxFill.pixelInset = new Rect(x, y, width, height);


		#endregion

		//TODO: replace this with a custom functionality thing for each
		//TODO: charge gauge for rocket sword?
		//TODO: simple meter for archer?
		//TODO: simple meter for style? (gets icicles at certain critical points?)
		//TODO: simple meter for sediment?
		#region Resource Bar
		float percentRes = myPlayerState.resource;
		GUITexture tempResBarFill = ResBarFill.GetComponent<GUITexture>();
		x = (int) tempResBarFill.pixelInset.x;
		y = (int) tempResBarFill.pixelInset.y ;
		height = (int) tempResBarFill.pixelInset.height;
		width = (int) Mathf.Max ( 1.0f, 100.0f * percentRes );
		tempResBarFill.pixelInset = new Rect(x, y, width, height);
		#endregion
	}
}
