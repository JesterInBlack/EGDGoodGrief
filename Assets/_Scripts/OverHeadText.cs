using UnityEngine;
using System.Collections;

public class OverHeadText : MonoBehaviour {

	#region vars
	private Player player;
	private Tutorial imageSource; //TODO: reclassify this.
	private SpriteRenderer spriteRenderer;

	private bool defer = false; //set to true to defer the normal update draw for 1 frame.
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		player = GetComponent<Player>();
		imageSource = GetComponent<Tutorial>();
		spriteRenderer = transform.Find ( "OverheadText" ).gameObject.GetComponent<SpriteRenderer>();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( defer == false )
		{
			SetOverheadText();
		}
		defer = false;
	}

	public void SetOverheadText()
	{
		defer = true;
		if ( GameState.isTutorial && ! imageSource.completed ) { return; } //don't conflict with tutorial messages
		
		spriteRenderer.enabled = true;
		if ( player.isDowned && player.HP >= player.maxHP * StaticData.percentHPNeededToRevive )
		{
			spriteRenderer.sprite = imageSource.aRevive;
		}
		else if ( player.isCarried )
		{
			spriteRenderer.sprite = imageSource.aDrop;
		}
		else if ( player.isCarrier )
		{
			spriteRenderer.sprite = imageSource.aDrop;
		}
		else if ( CanCarry() )
		{
			spriteRenderer.sprite = imageSource.aCarry;
		}
		else if ( player.state == "vampire" ) //oh the badness, it hurts.
		{
			spriteRenderer.sprite = imageSource.mashA;
		}
		else if ( player.contextualHealingAvailable )
		{
			spriteRenderer.sprite = imageSource.aHeal;
		}
		else
		{
			spriteRenderer.enabled = false;
		}
	}

	private bool CanCarry()
	{
		//returns true when press A to carry context command becomes available.
		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			if ( GameState.players[i].GetComponent<Player>().id != player.id ) //no self-carrying!
			{
				float x = GameState.players[i].gameObject.transform.position.x;
				float y = GameState.players[i].gameObject.transform.position.y;
				float myX = gameObject.transform.position.x;
				float myY = gameObject.transform.position.y;
				float dist = Mathf.Pow ( (x - myX) * (x - myX) + (y - myY) * (y - myY), 0.5f);
				if ( dist <= 1.0f ) //in range
				{
					if ( ! player.isCarried && ! player.isCarrier && ! player.isDowned) //you: valid state?
					{
						Player otherState = GameState.players[i].GetComponent<Player>();
						if ( ! otherState.isCarried && ! otherState.isCarrier && otherState.isDowned ) //them: valid state?
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
}
