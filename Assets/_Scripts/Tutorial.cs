using UnityEngine;
using System.Collections;

public class Tutorial : MonoBehaviour 
{

	#region vars
	public Sprite textYouDied;
	public Sprite textHowUnfortunate;
	public Sprite textCrawl;
	public Sprite textWait;

	public Sprite lsMove;

	public Sprite bumperSwap;
	public Sprite ltItem;

	public Sprite aRevive;
	public Sprite aCarry;
	public Sprite aDrop;
	public Sprite aHeal;

	public Sprite aim;
	public Sprite mashA;

	public Sprite rtCharge;
	public Sprite rtSandstorm;
	public Sprite rtFire;

	public Sprite bParry;
	public Sprite bCharge;
	public Sprite bStoneSkin;
	public Sprite bDodge;
	public Sprite bFlip;

	public Sprite xSlash;
	public Sprite xCharge;
	public Sprite xSpin;
	public Sprite xSpin2;
	public Sprite xPunch;
	public Sprite xPunch2;
	//public Sprite xBlowback;

	public Sprite ySlash;
	public Sprite yCharge;
	public Sprite yBlastOff;
	public Sprite yBlock;
	public Sprite yCounter;

	private Player player;
	private SpriteRenderer spriteRenderer;
	//private Sprite sprite;
	private int state = 0; //objective tracker.
	private float timer = 0.0f;

	[HideInInspector]
	public bool usedItem = false;
	[HideInInspector]
	public bool swappedItem = false;

	public bool completed = false;
	#endregion

	// Use this for pre-initialization
	void Awake ()
	{
		player = GetComponent<Player>();
		spriteRenderer = transform.Find ( "OverheadText" ).gameObject.GetComponent<SpriteRenderer>();
		//sprite = spriteRenderer.sprite;
	}

	// Use this for initialization
	void Start () 
	{
		if ( GameState.isTutorial )
		{
			spriteRenderer.enabled = true;
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( GameState.isTutorial )
		{
			if ( player.characterclass == CharacterClasses.KNIGHT )
			{
				RocketSwordTutorial();
			}
			else if ( player.characterclass == CharacterClasses.DEFENDER )
			{
				MonkTutorial ();
			}
			else if ( player.characterclass == CharacterClasses.ARCHER )
			{
				ArcherTutorial ();
			}
			else if ( player.characterclass == CharacterClasses.NINJA )
			{
				NinjaTutorial ();
			}
		}
	}

	private void RocketSwordTutorial()
	{
		if ( state == 0 ) //move
		{
			spriteRenderer.sprite = lsMove;
			if ( GetComponent<CustomController>().move_vec.magnitude > 0.0f )
			{
				state++;
			}
		}
		else if ( state == 1 ) //X: slash
		{
			spriteRenderer.sprite = xSlash;
			if ( player.state == "xnormal" ) //oh god, the badness!
			{
				state++;
			}
		}
		else if ( state == 2 ) //Y: slash
		{
			spriteRenderer.sprite = ySlash;
			if ( player.state == "ynormal" )
			{
				state++;
			}
		}
		else if ( state == 3 ) //B: parry
		{
			spriteRenderer.sprite = bParry;
			if ( player.state == "parry" )
			{
				state++;
			}
		}
		else if ( state == 3 ) //RT: charge
		{
			spriteRenderer.sprite = rtCharge;
			if ( player.state == "revcharge" && player.resource > 1.0f / 8.0f )
			{
				state++;
			}
		}
		else if ( state == 4 ) //Use item
		{
			spriteRenderer.sprite = ltItem;
			if ( usedItem )
			{
				state++;
			}
		}
		else if ( state == 5 ) //Swap items
		{
			spriteRenderer.sprite = bumperSwap;
			if ( swappedItem )
			{
				state++;
			}
		}
		else if ( state == 6 ) //X charge
		{
			if ( ! GetComponent<RocketSwordFunctions>().xCharged )
			{
				spriteRenderer.sprite = xCharge;
			}
			else if ( GetComponent<RocketSwordFunctions>().xCharged2 )
			{
				spriteRenderer.sprite = xSpin2; //for Andrew.
			}
			else
			{
				spriteRenderer.sprite = xSpin;
			}
			if ( player.state == "xsmash" )
			{
				state++;
			}
		}
		else if ( state == 7 ) //Y charge
		{
			if ( ! GetComponent<RocketSwordFunctions>().yCharged )
			{
				spriteRenderer.sprite = yCharge;
			}
			else
			{
				spriteRenderer.sprite = yBlastOff;
			}
			if ( player.state == "ysmash" )
			{
				state++;
			}
		}
		else if ( state == 8 ) //Filler state
		{
			timer += Time.deltaTime;
			if ( timer >= 1.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 9 ) //kill the player.
		{
			player.HP = 0.0f;
			spriteRenderer.sprite = textYouDied;
			timer += Time.deltaTime;
			if ( timer >= 3.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 10 ) //comment
		{
			spriteRenderer.sprite = textHowUnfortunate;
			timer += Time.deltaTime;
			if ( timer >= 3.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 11 ) //give crawl instructions
		{
			spriteRenderer.sprite = textCrawl;
			if ( player.HP >= 5.0f )
			{
				state++;
			}
		}
		else if ( state == 12 ) //tell player to chill
		{
			spriteRenderer.sprite = textWait;
			if ( player.HP >= player.maxHP * StaticData.percentHPNeededToRevive )
			{
				state++;
			}
		}
		else if ( state == 13 ) //tell player to revive
		{
			spriteRenderer.sprite = aRevive;
			if ( ! player.isDowned )
			{
				state++;
			}
		}
		else if ( state == 14 ) //WOOHOO! You did it! YAY!
		{
			spriteRenderer.enabled = false;
			completed = true;
			state++;
		}
	}

	private void MonkTutorial()
	{
		if ( state == 0 ) //move
		{
			spriteRenderer.sprite = lsMove;
			if ( GetComponent<CustomController>().move_vec.magnitude > 0.0f )
			{
				state++;
			}
		}
		else if ( state == 1 ) //X: punch
		{
			spriteRenderer.sprite = xPunch;
			if ( player.state == "xwinddown" ) //oh god, the badness!
			{
				state++;
			}
		}
		else if ( state == 2 ) //Y: slash
		{
			spriteRenderer.sprite = yBlock;
			if ( player.state == "ycharge" )
			{
				spriteRenderer.sprite = yCounter;
			}
			else if ( player.state == "ywinddown" )
			{
				state++;
			}
		}
		else if ( state == 3 ) //B: stoneskin
		{
			spriteRenderer.sprite = bStoneSkin;
			if ( player.isStoneSkin )
			{
				state++;
			}
		}
		else if ( state == 3 ) //RT: sandstorm
		{
			spriteRenderer.sprite = rtSandstorm;
			if ( player.state == "sandstorm" )
			{
				state++;
			}
		}
		else if ( state == 4 ) //Use item
		{
			spriteRenderer.sprite = ltItem;
			if ( usedItem )
			{
				state++;
			}
		}
		else if ( state == 5 ) //Swap items
		{
			spriteRenderer.sprite = bumperSwap;
			if ( swappedItem )
			{
				state++;
			}
		}
		else if ( state == 6 ) //X charge
		{
			if ( ! GetComponent<StoneFist>().xCharged )
			{
				spriteRenderer.sprite = xCharge;
			}
			else
			{
				spriteRenderer.sprite = xPunch2;
			}
			if ( player.state == "xwinddown" )
			{
				state++;
			}
		}
		else if ( state == 7 ) //RT charge
		{ 
			spriteRenderer.sprite = rtSandstorm;
			if ( player.state == "sandstorm" )
			{
				state++; 
			}
		} 
		else if ( state == 8 ) //Filler state
		{
			timer += Time.deltaTime;
			if ( timer >= 1.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 9 ) //kill the player.
		{
			player.HP = 0.0f;
			spriteRenderer.sprite = textYouDied;
			timer += Time.deltaTime;
			if ( timer >= 3.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 10 ) //comment
		{
			spriteRenderer.sprite = textHowUnfortunate;
			timer += Time.deltaTime;
			if ( timer >= 3.0f )
			{
				timer = 0.0f;
				state++;
			}
		}
		else if ( state == 11 ) //give crawl instructions
		{
			spriteRenderer.sprite = textCrawl;
			if ( player.HP >= 5.0f )
			{
				state++;
			}
		}
		else if ( state == 12 ) //tell player to chill
		{
			spriteRenderer.sprite = textWait;
			if ( player.HP >= player.maxHP * StaticData.percentHPNeededToRevive )
			{
				state++;
			}
		}
		else if ( state == 13 ) //tell player to revive
		{
			spriteRenderer.sprite = aRevive;
			if ( ! player.isDowned )
			{
				state++;
			}
		}
		else if ( state == 14 ) //WOOHOO! You did it! YAY!
		{
			spriteRenderer.enabled = false;
			completed = true;
			state++;
		}
	}

	private void ArcherTutorial()
	{
	}

	private void NinjaTutorial()
	{
	}
}
