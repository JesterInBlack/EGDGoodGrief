using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public enum MenuScreens { START, MAIN_MENU, CHARACTER_SELECTION, ITEM_SELECTION };

public class PreGameMenu : MonoBehaviour 
{
	//TODO: add sound: yes/no/scroll
	#region vars
	public MenuDataSaver menuDataSaver;

	GamePadState[] currentGamePadState = new GamePadState[4];
	GamePadState[] prevState = new GamePadState[4];
	float xScale = 1.0f;  //deal with screen resizing
	float yScale = 1.0f;  //deal with screen resizing

	private bool[] playersConnected = { true, false, false, false };
	private Color[] playerColors = new Color[4];
	private CharacterClasses[] playerClasses = { CharacterClasses.KNIGHT, CharacterClasses.KNIGHT, CharacterClasses.KNIGHT, CharacterClasses.KNIGHT };
	private ItemName[,] playerItems = new ItemName[4, 3];
	private bool tutorial = false;                         //whether or not to go to the tutorial scene upon completion.
	private MenuScreens currentScreen = MenuScreens.START; //Title, Character, Item

	public Texture2D titleBackground;

	public Texture2D mainMenuBackground;                                    //Background          set in inspector
	public Texture2D mainMenuPlayButton;                                    //Button image        set in inspector
	public Texture2D mainMenuPlayButtonGray;                                //Button image (gray) set in inspector
	private float playButtonLerp = 1.0f;                                    //Lerp
	public Texture2D mainMenuOptionsButton;                                 //Button image        set in inspector
	public Texture2D mainMenuOptionsButtonGray;                             //Button image (gray) set in inspector
	private float optionsButtonLerp = 0.0f;                                 //Lerp
	public Texture2D mainMenuTutorialButton;                                //Button image        set in inspector
	public Texture2D mainMenuTutorialButtonGray;                            //Button image (gray) set in inspector
	private float tutorialButtonLerp = 0.0f;                                //Lerp
	public Texture2D mainMenuCreditsButton;                                 //Button image        set in inspector
	public Texture2D mainMenuCreditsButtonGray;                             //Button image (gray) set in inspector
	private float creditsButtonLerp = 0.0f;                                 //Lerp
	private int mainMenuIndex = 0;
	private const int MAIN_MENU_BUTTON_COUNT = 4;

	public Texture2D characterBackground;
	public Texture2D[] playerHoverIcons = new Texture2D[4];                 //"hover" selector icons
	public Texture2D/*[]*/ playerLockIcon;// = new Texture2D[4];            //"locked in" selector icon
	//public Texture2D[] playerGrayHoverIcons = new Texture2D[4];           //"hover" : impossible selector icon
	public Texture2D[] playerCharFrame = new Texture2D[4];                  //frame for character selection
	public Texture2D[] playerItemFrame = new Texture2D[4];                  //frame for item selection
	public Vector2[] charCoords = new Vector2[4];                           //coordinates of the character selection square
	public Vector2[] charSelectorOffsets = new Vector2[4];                  //offsets for player indicators (relative to selection square)
	public Rect[] charTextPos = new Rect[4];                                //offset + size for player text
	private string[] charClasses = { "Knight", "Monk", "Archer", "Ninja" };
	public GUIStyle charGUIStyle;                                           //font for player text
	private int[] charSelection = new int[4];                               //index of player's selection
	private bool[] charLockedIn = {false, false, false, false};             //if the player has confirmed their choice
	private const float charLockinDelay = 3.0f;                             //3 ... 2... 1 ... GO!
	private float charLockinTimer = 3.0f;
	private const int CLASS_COUNT = 2;                                      //number of classes

	public Texture2D itemBackground;
	private Texture2D[] itemIcons = new Texture2D[6];      //item icons
	private ItemName[] itemNames = { ItemName.PHEROMONE_JAR, ItemName.STOPWATCH, ItemName.VAMPIRE_FANG, 
		ItemName.AURA_OFFENSE, ItemName.AURA_DEFENSE, ItemName.AURA_REGEN };
	public GUIStyle itemTextGUIStyle;
	private string[] itemNameText = new string[6];
	private string[] itemText = new string[6];
	private Rect[] itemTextRect = new Rect[4];             //coordinates and dimensions of the item description text.
	public Vector2 itemCoords;                             //coordinates of the item selection square (0,0)
	public Vector2[] itemSelectorOffsets = new Vector2[4]; //offsets for player indicators
	private int[,] itemSelection = new int[4, 3];          //indecies of character's selections
	private int[] itemIndex = new int[4];                  //index of character's current selection
	private int[] itemSelectionCount = new int[4];         //
	private const int ITEMS_PER_ROW = 6;                   //number of items per row
	private const int ITEM_COUNT = 6;                      //number of items in total
	private const float itemLockinDelay = 3.0f;            //3 ... 2 ... 1 ... GO!
	private float itemLockinTimer = 3.0f;

	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		ItemImages.Start (); //Load item images!
	}

	// Use this for initialization
	void Start () 
	{
		playerColors[0] = new Color( 1.0f, 0.0f, 0.0f, 1.0f );
		playerColors[1] = new Color( 0.0f, 0.0f, 1.0f, 1.0f );
		playerColors[2] = new Color( 0.0f, 1.0f, 0.0f, 1.0f );
		playerColors[3] = new Color( 1.0f, 1.0f, 0.0f, 1.0f );

		xScale = Screen.width / 1440.0f;
		yScale = Screen.height / 900.0f;

		#region Initialize_Items
		for ( int i = 0; i < 4; i++ )
		{
			for ( int j = 0; j < 3; j++ )
			{
				playerItems[i, j] = ItemName.STOPWATCH;
				itemSelection[i, j] = -1; //set index to -1.
			}
			itemSelectionCount[i] = 0; //set number of selected items to 0 for all players
		}

		//100 x 100 icons
		//131, 39 start: +120, +120, 12 slots: 2 rows of 6.
		itemCoords = new Vector2( 131.0f * 1440.0f / 960.0f, 39.0f * 900.0f / 600.0f );
		itemSelectorOffsets[0] = new Vector2(  0.0f, 0.0f );
		itemSelectorOffsets[1] = new Vector2( 52.0f * 1440.0f / 960.0f, 0.0f);
		itemSelectorOffsets[2] = new Vector2(  0.0f, 52.0f * 1440.0f / 960.0f);
		itemSelectorOffsets[3] = new Vector2( 52.0f * 1440.0f / 960.0f, 52.0f * 1440.0f / 960.0f);

		//Set up images.
		itemIcons[0] = ItemImages.phermoneJar;
		itemIcons[1] = ItemImages.stopWatch;
		itemIcons[2] = ItemImages.vampireFang;
		itemIcons[3] = ItemImages.auraOffense;
		itemIcons[4] = ItemImages.auraDefense;
		itemIcons[5] = ItemImages.auraHeal;

		itemText[0] = "Pheromone Jar: \nThrow a jar of pheromones which draws the boss' fire to any players it splashes on.";
		itemText[1] = "Stop Watch: \nDilate time, allowing you to move faster than the boss and any allies stuck moving at normal speed.";
		itemText[2] = "Vampire Fang: \nDrain life from friends or foes.";
		itemText[3] = "Offense Up Aura: \nIncrease attack for you and your allies. If an ally messes with you, they lose the buff.";
		itemText[4] = "Defense Up Aura: \nIncrease defense for you and your allies. If an ally messes with you, they lose the buff.";
		itemText[5] = "Healing Aura: \nConstantly heal you and your allies. If an ally messes with you, they lose the buff.";

		itemNameText[0] = "Pheromone Jar";
		itemNameText[1] = "Stop Watch";
		itemNameText[2] = "Vampire Fang";
		itemNameText[3] = "Offense Up Aura";
		itemNameText[4] = "Defense Up Aura";
		itemNameText[5] = "Healing Aura";

		itemTextRect[0] = new Rect( 124.0f * 1440.0f / 960.0f, 300.0f * 900.0f / 600.0f, 169.0f * 1440.0f / 960.0f, 240.0f * 900.0f / 600.0f );
		itemTextRect[1] = new Rect( 300.0f * 1440.0f / 960.0f, 300.0f * 900.0f / 600.0f, 169.0f * 1440.0f / 960.0f, 240.0f * 900.0f / 600.0f );
		itemTextRect[2] = new Rect( 484.0f * 1440.0f / 960.0f, 300.0f * 900.0f / 600.0f, 169.0f * 1440.0f / 960.0f, 240.0f * 900.0f / 600.0f );
		itemTextRect[3] = new Rect( 666.0f * 1440.0f / 960.0f, 300.0f * 900.0f / 600.0f, 169.0f * 1440.0f / 960.0f, 240.0f * 900.0f / 600.0f );
		#endregion

		#region CharacterSelectionScreen
		//Extracted offsets from the image file itself.
		//coordinates for the text.
		charTextPos[0] = new Rect( 115.0f, 470.0f, 292.0f - 115.0f, 52.0f );
		charTextPos[1] = new Rect( 300.0f, 470.0f, 475.0f - 300.0f, 52.0f );
		charTextPos[2] = new Rect( 485.0f, 470.0f, 659.0f - 485.0f, 52.0f );
		charTextPos[3] = new Rect( 666.0f, 470.0f, 842.0f - 666.0f, 52.0f );

		//coordinates of the 4 character icon boxes
		charCoords[0] = new Vector2( 120.0f, 65.0f );
		charCoords[1] = new Vector2( 305.0f, 65.0f );
		charCoords[2] = new Vector2( 487.0f, 65.0f );
		charCoords[3] = new Vector2( 671.0f, 65.0f );

		//corners for 32x32 image (as relative coords)
		charSelectorOffsets[0] = new Vector2(   0.0f, 0.0f );
		charSelectorOffsets[1] = new Vector2( 283.0f - 120.0f - 48.0f, 0.0f );
		charSelectorOffsets[2] = new Vector2(   0.0f, 230.0f - 65.0f - 48.0f );
		charSelectorOffsets[3] = new Vector2( 283.0f - 120.0f - 48.0f, 230.0f - 65.0f - 48.0f );

		//Scale image 960 x 600 -> 1440 x 900
		for ( int i = 0; i < 4; i++ )
		{
			charTextPos[i].x      = charTextPos[i].x * 1440.0f / 960.0f;
			charTextPos[i].y      = charTextPos[i].y * 900.0f / 600.0f;
			charTextPos[i].width  = charTextPos[i].width * 1440.0f / 960.0f;
			charTextPos[i].height = charTextPos[i].height * 900.0f / 600.0f;

			charCoords[i].x = charCoords[i].x * 1440.0f / 960.0f;
			charCoords[i].y = charCoords[i].y * 900.0f / 600.0f;

			charSelectorOffsets[i].x = charSelectorOffsets[i].x * 1440.0f / 960.0f;
			charSelectorOffsets[i].y = charSelectorOffsets[i].y * 900.0f / 600.0f;
		}
		#endregion
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Debug.Log ( GamePad.GetState (PlayerIndex.One).ThumbSticks.Left.X + ", " + GamePad.GetState (PlayerIndex.One).ThumbSticks.Left.Y);
		//TITLE: press start!

		for ( int i = 0; i < 4; i++ )
		{
			currentGamePadState[i] = GamePad.GetState ( (PlayerIndex) i );
		}


		if ( currentScreen == MenuScreens.START )
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( onAPressed( currentGamePadState[i], prevState[i] ) )
				{
					currentScreen = MenuScreens.MAIN_MENU;
				}
			}
		}
		else if ( currentScreen == MenuScreens.MAIN_MENU )
		{
			#region MainMenu
			if ( mainMenuIndex == 0 ) { playButtonLerp = Mathf.Min ( 1.0f, playButtonLerp + Time.deltaTime * 3.0f ); }
			else {                      playButtonLerp = Mathf.Max ( 0.0f, playButtonLerp - Time.deltaTime * 3.0f ); }
			if ( mainMenuIndex == 1 ) { tutorialButtonLerp = Mathf.Min ( 1.0f, tutorialButtonLerp + Time.deltaTime * 3.0f ); }
			else {                      tutorialButtonLerp = Mathf.Max ( 0.0f, tutorialButtonLerp - Time.deltaTime * 3.0f ); }
			if ( mainMenuIndex == 2 ) { optionsButtonLerp = Mathf.Min ( 1.0f, optionsButtonLerp + Time.deltaTime * 3.0f ); }
			else {                      optionsButtonLerp = Mathf.Max ( 0.0f, optionsButtonLerp - Time.deltaTime * 3.0f ); }
			if ( mainMenuIndex == 3 ) { creditsButtonLerp = Mathf.Min ( 1.0f, creditsButtonLerp + Time.deltaTime * 3.0f ); }
			else {                      creditsButtonLerp = Mathf.Max ( 0.0f, creditsButtonLerp - Time.deltaTime * 3.0f ); }

			if ( onAPressed( currentGamePadState[0], prevState[0] ) )
			{
				//do stuff depending on what's selected
				if ( mainMenuIndex == 0 ) //PLAY
				{
					GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuConfirm );
					currentScreen = MenuScreens.CHARACTER_SELECTION;
					for ( int i = 0; i < 4; i++ )
					{
						if ( GamePad.GetState ( (PlayerIndex) i ).IsConnected )
						{
							playersConnected[i] = true;
						}
					}
				}
				else if ( mainMenuIndex == 1 ) //TUTORIAL
				{
					GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuConfirm );
					tutorial = true;
					menuDataSaver.tutorial = tutorial; //write
					GameState.isTutorial = tutorial; //write
					currentScreen = MenuScreens.CHARACTER_SELECTION;
					for ( int i = 0; i < 4; i++ )
					{
						if ( GamePad.GetState ( (PlayerIndex) i ).IsConnected )
						{
							playersConnected[i] = true;
						}
					}
				}
				else if ( mainMenuIndex == 2 ) //OPTIONS
				{
				}
				else if ( mainMenuIndex == 3 ) //CREDITS
				{
					GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuConfirm );
					Application.LoadLevel ( "Credits" );
				}
			}
			if ( onDownPressed ( currentGamePadState[0], prevState[0] ) )
			{
				mainMenuIndex++;
				mainMenuIndex = mainMenuIndex % MAIN_MENU_BUTTON_COUNT;
			}
			if ( onUpPressed ( currentGamePadState[0], prevState[0] ) )
			{
				mainMenuIndex--;
				mainMenuIndex = (mainMenuIndex + MAIN_MENU_BUTTON_COUNT) % MAIN_MENU_BUTTON_COUNT;
			}
			#endregion
		}
		else if ( currentScreen == MenuScreens.CHARACTER_SELECTION )
		{
			#region CharacterSelection
			//if everyone's locked in (321) (press start to continue)
			for ( int i = 0; i < 4; i++ )
			{
				if ( onAPressed( currentGamePadState[i], prevState[i] ) )
				{
					if ( ! playersConnected[i] ) //A to play
					{
						playersConnected[i] = true;
					}
					else if ( ! charLockedIn[i] ) //A to confirm choice
					{
						charLockedIn[i] = true;
						GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuConfirm );
					}
				}
				if ( onBPressed( currentGamePadState[i], prevState[i] ) )
				{
					if ( playersConnected[i] )
					{
						if ( charLockedIn[i] ) //B to change choice
						{
							charLockedIn[i] = false;
						}
						else //B to not play
						{
							playersConnected[i] = false;
						}
					}
					else if ( ! playersConnected[i] && i == 0 ) //player 1 pressed B to go back.
					{
						currentScreen = MenuScreens.MAIN_MENU;
						GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuBack );
					}
				}
				if ( onLeftPressed( currentGamePadState[i], prevState[i] ) )
				{
					if ( playersConnected[i] )
					{
						if ( ! charLockedIn[i] ) //change index
						{
							charSelection[i] --;
							charSelection[i] = (charSelection[i] + CLASS_COUNT) % CLASS_COUNT;
						}
						else //Locked in
						{
							GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuCancel );
						}
					}
				}
				if ( onRightPressed( currentGamePadState[i], prevState[i] ) )
				{
					if ( playersConnected[i] )
					{
						if ( ! charLockedIn[i] ) //change index
						{
							charSelection[i] ++;
							charSelection[i] = charSelection[i] % CLASS_COUNT;
						}
						else //Locked in
						{
							GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuCancel );
						}
					}
				}
			}

			bool allLockedIn = true;
			//allLockedIn = allLockedIn && charLockedIn[0] && charLockedIn[1] && charLockedIn[2] && charLockedIn[3];
			/**/
			allLockedIn = allLockedIn && 
				( charLockedIn[0] || ! playersConnected[0] ) && 
				( charLockedIn[1] || ! playersConnected[1] ) && 
				( charLockedIn[2] || ! playersConnected[2] ) && 
				( charLockedIn[3] || ! playersConnected[3] );
			/**/
			if ( allLockedIn )
			{
				charLockinTimer -= Time.deltaTime;
			}
			else
			{
				charLockinTimer = charLockinDelay;
			}

			if ( charLockinTimer <= 0.0f )
			{
				//Parse selection into class data.
				for ( int i = 0; i < 4; i++ )
				{
					if ( charSelection[i] == 0 )
					{
						playerClasses[i] = CharacterClasses.KNIGHT;
					}
					else if ( charSelection[i] == 1 )
					{
						playerClasses[i] = CharacterClasses.DEFENDER;
					}
					else if ( charSelection[i] == 2 )
					{
						playerClasses[i] = CharacterClasses.ARCHER;
					}
					else if ( charSelection[i] == 3 )
					{
						playerClasses[i] = CharacterClasses.NINJA;
					}
				}
				//TODO: write data
				currentScreen = MenuScreens.ITEM_SELECTION;
			}
			#endregion
		}
		else if ( currentScreen == MenuScreens.ITEM_SELECTION )
		{
			#region ItemSelection
			//if everyone's locked in (321)
			for ( int i = 0; i < 4; i++ )
			{
				if ( onLeftPressed( currentGamePadState[i], prevState[i] ) )
				{
					itemIndex[ i ]--;
					itemIndex[ i ] = ( itemIndex[i] + ITEM_COUNT ) % ITEM_COUNT;
				}
				if ( onRightPressed( currentGamePadState[i], prevState[i] ) )
				{
					itemIndex[ i ]++;
					itemIndex[ i ] = ( itemIndex[i] + ITEM_COUNT ) % ITEM_COUNT;
				}
				if ( onUpPressed( currentGamePadState[i], prevState[i] ) )
				{
					itemIndex[ i ] -= ITEMS_PER_ROW;
					itemIndex[ i ] = ( itemIndex[i] + ITEM_COUNT ) % ITEM_COUNT;
				}
				if ( onDownPressed( currentGamePadState[i], prevState[i] ) )
				{
					itemIndex[ i ] += ITEMS_PER_ROW;
					itemIndex[ i ] = ( itemIndex[i] + ITEM_COUNT ) % ITEM_COUNT;
				}
				if ( onAPressed ( currentGamePadState[i], prevState[i] ) )
				{
					//if open item slot, save index
					if ( itemSelectionCount[i] < 3 )
					{
						itemSelection[ i, itemSelectionCount[i] ] = itemIndex[i];
						itemSelectionCount[i]++;
						GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuConfirm );
					}
					else
					{
						GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuCancel );
					}
				}
				if ( onBPressed ( currentGamePadState[i], prevState[i] ) )
				{
					//if this player has selected no items, go back a screen.
					bool goBack = true;
					for ( int j = 0; j < 3; j++ )
					{
						if ( itemSelection[ i, j ] != -1 )
						{
							goBack = false;
						}
					}
					if ( goBack )
					{
						GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuBack );
						currentScreen = MenuScreens.CHARACTER_SELECTION;
						return;
					}

					//if this item has been selected, free it.
					for ( int j = 0; j < 3; j++ )
					{
						if ( itemSelection[ i, j ] == itemIndex[i] )
						{
							itemSelection[ i, j ] = itemSelection[ i, itemSelectionCount[i] - 1 ]; //swap with last element
							itemSelection[ i, itemSelectionCount[i] - 1 ] = -1;                    //remove last element
							itemSelectionCount[i]--;                                               //decrement tracker
							GetComponent<AudioSource>().PlayOneShot ( SoundStorage.MenuBack );
						}
					}
				}
			}

			//for each connected player, if their selection count == 3.
			bool allLockedin = true;
			for ( int i = 0; i < 4; i++ )
			{
				if ( itemSelectionCount[i] == 3 || ! playersConnected[i] )
				{
					//full item selections, player not connected.
				}
				else
				{
					allLockedin = false;
				}
			}

			if ( allLockedin )
			{
				itemLockinTimer -= Time.deltaTime;
			}
			else
			{
				itemLockinTimer = itemLockinDelay;
			}

			if ( itemLockinTimer <= 0.0f )
			{
				playersConnected.CopyTo( menuDataSaver.playersConnected, 0 );
				for ( int i = 0; i < 4; i++ )
				{
					for ( int j = 0; j < 3; j++ )
					{
						if ( itemSelection[i, j] != -1 )
						{
							menuDataSaver.playerItems[i, j] = itemNames[ itemSelection[i, j] ];
						}
					}
				}
				playerClasses.CopyTo( menuDataSaver.playerClasses, 0 );
				if ( tutorial )
				{
					Application.LoadLevel( "Tutorial" );
				}
				else
				{
					Application.LoadLevel( "Master" );
				}
			}
			#endregion
		}
		//Item select
		//-------------------------------------------------------------------
		//hovering:
		//rect for cursor, scale it (0-4, decreasing size)
		//number

		//selected:
		//place selector icon.
		//b to remove

		//b for back.
		//wait for everyone to lock in + record all player's items. (use enum)
		 

		for ( int i = 0; i < 4; i++ )
		{
			prevState[i] = currentGamePadState[i];
		}
	}

	void OnGUI ()
	{
		if ( currentScreen == MenuScreens.START )
		{
			//Draw background
			GUI.DrawTexture ( new Rect( 0.0f, 0.0f, 1440.0f * xScale, 900.0f * yScale), titleBackground );
		}
		else if ( currentScreen == MenuScreens.MAIN_MENU )
		{
			#region MainMenu
			//Draw background
			GUI.DrawTexture ( new Rect( 0.0f, 0.0f, 1440.0f * xScale, 900.0f * yScale), mainMenuBackground );

			float x = (Screen.width - 200.0f) / 2.0f;
			float y = (Screen.height - 60.0f) / 2.0f - 160.0f;
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuPlayButtonGray );
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, playButtonLerp );
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuPlayButton );
			GUI.color = Color.white;

			y += 80.0f;
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuTutorialButtonGray );
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, tutorialButtonLerp );
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuTutorialButton );
			GUI.color = Color.white;

			y += 80.0f;
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuOptionsButtonGray );
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, optionsButtonLerp );
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuOptionsButton );
			GUI.color = Color.white;

			y += 80.0f;
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuCreditsButtonGray );
			GUI.color = new Color( 1.0f, 1.0f, 1.0f, creditsButtonLerp );
			GUI.DrawTexture ( new Rect( x, y, 200.0f, 60.0f ), mainMenuCreditsButton );
			GUI.color = Color.white;
			#endregion
		}
		else if ( currentScreen == MenuScreens.CHARACTER_SELECTION )
		{
			#region CharacterSelection
			//Draw background
			GUI.DrawTexture ( new Rect( 0.0f, 0.0f, 1440.0f * xScale, 900.0f * yScale), characterBackground );

			for ( int i = 0; i < 4; i++ )
			{
				float x, y; //temp vars for readability

				//Display lock?
				if ( charLockedIn[i] && playersConnected[i] )
				{
					GUI.color = playerColors[ i ];
					x = ( charCoords[ charSelection[ i ] ].x + charSelectorOffsets[ i ].x ) * xScale - 4.0f;
					y = ( charCoords[ charSelection[ i ] ].y + charSelectorOffsets[ i ].y ) * yScale - 24.0f;
					GUI.DrawTexture ( new Rect( x, y, 56.0f, 72.0f ), playerLockIcon );
					GUI.color = Color.white;
				}

				//Display selector icon
				if ( playersConnected[i] )
				{
					x = ( charCoords[ charSelection[ i ] ].x + charSelectorOffsets[ i ].x ) * xScale;
					y = ( charCoords[ charSelection[ i ] ].y + charSelectorOffsets[ i ].y ) * yScale;
					GUI.DrawTexture ( new Rect( x, y, 48.0f, 48.0f ), playerHoverIcons[ i ] );
				}


				//display class selection text
				charGUIStyle.normal.textColor = playerColors[i];
				Rect tempRect = new Rect( charTextPos[ i ].x * xScale, 
				                          charTextPos[ i ].y * yScale, 
				                          charTextPos[ i ].width * xScale, 
				                          charTextPos[ i ].height * yScale );
				if ( playersConnected[i] )
				{
					GUI.Label ( tempRect, charClasses[ charSelection[ i ] ], charGUIStyle );
				}
				else
				{
					if ( GamePad.GetState ( (PlayerIndex) i ).IsConnected )
					{
						GUI.Label ( tempRect, "Press A!", charGUIStyle );
					}
				}
			}
			charGUIStyle.normal.textColor = Color.black;
			#endregion
		}
		else if ( currentScreen == MenuScreens.ITEM_SELECTION )
		{
			#region ItemSelection
			//Draw background
			GUI.DrawTexture ( new Rect( 0.0f, 0.0f, 1440.0f * xScale, 900.0f * yScale), itemBackground );

			float index, x, y;
			int row, column;

			//Draw items
			for ( int i = 0; i < 6; i++ )
			{
				row     = (int)(i / 6);
				column  = (int)(i % 6);
				x       = ( itemCoords.x + column * 120.0f * 1440.0f / 960.0f) * xScale;
				y       = ( itemCoords.y + row * 120.0f * 900.0f / 600.0f ) * yScale;
				float width = 100.0f * 1440.0f / 960.0f * xScale;
				float height = 100.0f * 900.0f / 600.0f * yScale;
				GUI.DrawTexture ( new Rect( x + width / 2.0f - 16.0f, y + height / 2.0f - 16.0f, 32.0f, 32.0f ), itemIcons[i] );
			}

			//Draw player locks + frames.
			for ( int i = 0; i < 4; i++ )
			{
				if ( playersConnected[i] )
				{
					//draw frame around currently hovering over index.
					index   = itemIndex[i];
					row     = (int)(index / 6);
					column  = (int)(index % 6);
					x       = ( itemCoords.x + column * 120.0f * 1440.0f / 960.0f) * xScale;
					y       = ( itemCoords.y + row * 120.0f * 900.0f / 600.0f ) * yScale;
					GUI.DrawTexture ( new Rect(x, y, 100.0f * 1440.0f / 960.0f * xScale, 100.0f * 900.0f / 600.0f * yScale), 
					                  playerItemFrame[i], ScaleMode.StretchToFill );

					x = ( itemCoords.x + column * 120.0f * 1440.0f / 960.0f + itemSelectorOffsets[i].x ) * xScale;
					y = ( itemCoords.y + row * 120.0f * 900.0f / 600.0f + itemSelectorOffsets[i].y ) * yScale;
					GUI.DrawTexture ( new Rect(x, y, 48.0f, 48.0f), playerHoverIcons[i] );
				}
			}

			//The reason this is done in two loops is so the draw order will be correct.

			for ( int i = 0; i < 4; i++ )
			{	
				if ( playersConnected[i] )
				{
					//Draw selected items
					for ( int j = 0; j < 3; j++ )
					{
						//itemSelection[ i, j ]; //draw lock + hover
						if ( itemSelection[ i, j ] != -1 )
						{
							index  = itemSelection[ i, j ];
							row    = (int)(index / 6);
							column = (int)(index % 6);
							
							x = ( itemCoords.x + column * 120.0f * 1440.0f / 960.0f + itemSelectorOffsets[i].x ) * xScale;
							y = ( itemCoords.y + row * 120.0f * 900.0f / 600.0f + itemSelectorOffsets[i].y ) * yScale;
							GUI.color = playerColors[i];
							GUI.DrawTexture ( new Rect(x - 4.0f, y - 24.0f, 56.0f, 72.0f), playerLockIcon );
							GUI.color = Color.white;
							GUI.DrawTexture ( new Rect(x, y, 48.0f, 48.0f), playerHoverIcons[i] );
						}
					}
					//Draw description text for the currently hovered over item.
					itemTextGUIStyle.normal.textColor = playerColors[i];
					for ( int j = 0; j < 3; j++ )
					{
						string temp = "[Pick an item!]";
						if ( itemSelection[i, j] > -1 )
						{
							temp = itemNameText[ itemSelection[i, j] ];
						}
						GUI.Label (  new Rect( itemTextRect[ i ].x, itemTextRect[ i ].y - 32.0f * (j + 1), itemTextRect[ i ].width, itemTextRect[ i ].height ), temp, itemTextGUIStyle );
					}

					itemTextGUIStyle.normal.textColor = new Color( 0.0f, 0.0f, 0.0f, 1.0f );
					GUI.Label ( itemTextRect[ i ], itemText[ itemIndex[ i ] ], itemTextGUIStyle );
				}
			}
			#endregion
		}
	}

	#region Buttons
	bool onLeftPressed( GamePadState state, GamePadState prev )
	{
		//returns true when left is pressed
		if ( state.ThumbSticks.Left.X <= -0.5f && Mathf.Abs( prev.ThumbSticks.Left.X ) < 0.5f )
		{
			return true;
		}
		return false;
	}

	bool onRightPressed( GamePadState state, GamePadState prev )
	{
		//returns true when right is pressed
		if ( state.ThumbSticks.Left.X >= 0.5f && Mathf.Abs( prev.ThumbSticks.Left.X ) < 0.5f )
		{
			return true;
		}
		return false;
	}

	bool onUpPressed( GamePadState state, GamePadState prev )
	{
		//returns true when up is pressed
		if ( state.ThumbSticks.Left.Y >= 0.5f && Mathf.Abs( prev.ThumbSticks.Left.Y ) < 0.5f )
		{
			return true;
		}
		return false;
	}

	bool onDownPressed( GamePadState state, GamePadState prev )
	{
		//returns true when down is pressed
		if ( state.ThumbSticks.Left.Y <= -0.5f && Mathf.Abs( prev.ThumbSticks.Left.Y ) < 0.5f )
		{
			return true;
		}
		return false;
	}

	bool onAPressed( GamePadState state, GamePadState prev )
	{
		//returns true when A is pressed
		if ( state.Buttons.A == ButtonState.Pressed && prev.Buttons.A == ButtonState.Released )
		{
			return true;
		}
		return false;
	}

	bool onBPressed( GamePadState state, GamePadState prev )
	{
		//returns true when B is pressed
		if ( state.Buttons.B == ButtonState.Pressed && prev.Buttons.B == ButtonState.Released )
		{
			return true;
		}
		return false;
	}
	#endregion
}
