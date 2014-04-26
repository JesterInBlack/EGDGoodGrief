using UnityEngine;
using System.Collections;

public class PostGameScore : MonoBehaviour 
{
	#region vars
	public GameObject[] scorebars = new GameObject[4];              //set in inspector
	public GameObject[] playerSprites = new GameObject[4];          //set in inspector (need to set sprite renderer, + animate it, + jump, + move with scorebar)
	private CharacterClasses[] charClasses = new CharacterClasses[4];
	public RuntimeAnimatorController[] knightAnis = new RuntimeAnimatorController[4];
	public RuntimeAnimatorController[] monkAnis = new RuntimeAnimatorController[4];

	public GameObject[] spotLights = new GameObject[4];             //set in inspector
	public GameObject[] totalScoreText = new GameObject[4];         //set in inspector
	public GameObject[] objScoreText = new GameObject[4];           //set in inspector
	public ScoreDetails[] playerScoreDetails = new ScoreDetails[4]; //pulled from objects?
	public GameObject[] confettiEmitters = new GameObject[5];       //set in inspector
	public TextMesh objectiveText;                                  //set in inspector

	private float[] currentBarPercent = new float[4];
	private float[] nextBarPercent = new float[4];
	
	private float[] totalScores = new float[4];
	private float[] currentObjectiveScores = new float[4];
	private float[] nextTotalScores = new float[4];

	//2D arrays don't want to be jagged, ergo this badness.
	private SubScore[] objectiveScoresP1 = new SubScore[5];
	private SubScore[] objectiveScoresP2 = new SubScore[5];
	private SubScore[] objectiveScoresP3 = new SubScore[5];
	private SubScore[] objectiveScoresP4 = new SubScore[5];

	private int objectiveScoreIndex = 0;
	private const int NUMBER_OF_OBJECTIVES = 6; //# of subscores that go into the total score.
	private float t;                            //periodic timer for jumping animation, also counts seconds since "done".
	private float stateTimer = 0.0f;            //timer for state transitions
	private int state = 0;                      //state tracker
	private bool done = false;                  //if the state loop is done
	private bool[] winner = {false, false, false, false};               
	#endregion

	//Use this for pre-initialization
	void Awake ()
	{
		//pull PLAYER COUNT (+ center scorebars appropriately)
		//pull PLAYER CLASS (+ set animators + sprites appropriately) (copy from pre-game)

		playerScoreDetails[0] = GameObject.Find ( "P1ScoreData" ).GetComponent<ScorePasser>().scoreDetails;
		playerScoreDetails[1] = GameObject.Find ( "P2ScoreData" ).GetComponent<ScorePasser>().scoreDetails;
		playerScoreDetails[2] = GameObject.Find ( "P3ScoreData" ).GetComponent<ScorePasser>().scoreDetails;
		playerScoreDetails[3] = GameObject.Find ( "P4ScoreData" ).GetComponent<ScorePasser>().scoreDetails;

		objectiveScoresP1 = playerScoreDetails[0].GetObjectiveScores();
		objectiveScoresP2 = playerScoreDetails[1].GetObjectiveScores();
		objectiveScoresP3 = playerScoreDetails[2].GetObjectiveScores();
		objectiveScoresP4 = playerScoreDetails[3].GetObjectiveScores();

		for ( int i = 0; i < 4; i++ )
		{
			charClasses[i] = playerScoreDetails[i].GetCharacterClass();
			totalScores[i] = 0.0f;
			currentObjectiveScores[i] = 0.0f;
			currentBarPercent[i] = 0.0f;
		}

		//We are finished reading the data.
		//We need to destroy the score objects. (so that when we start a new game, we don't run into issues)
		Destroy ( GameObject.Find ( "P1ScoreData" ) );
		Destroy ( GameObject.Find ( "P2ScoreData" ) );
		Destroy ( GameObject.Find ( "P3ScoreData" ) );
		Destroy ( GameObject.Find ( "P4ScoreData" ) );
	}

	// Use this for initialization
	void Start () 
	{
		scorebars[0].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.0f, 0.0f, 0.5f );
		scorebars[1].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 0.0f, 1.0f, 0.5f );
		scorebars[2].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 1.0f, 0.0f, 0.5f );
		scorebars[3].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 0.0f, 0.5f );
		InitializeScoreBars (); //initialize them.
		GetComponent<AudioSource>().Play ();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( done ) { t += Time.deltaTime; }
		DoStateStuff();
		MoveScoreBars();
		MakeWinnersJump();

		if ( t > 6.0f ) { Application.LoadLevel( "MainMenu" ); };
	}

	private float GetMaxScore ()
	{
		//Gets the highest total score. (to correctly scale bars)
		float highest = 1.0f;
		for ( int i = 0; i < 4; i++ )
		{
			if ( totalScores[i] > highest )
			{
				highest = totalScores[i];
			}
		}
		return highest;
	}

	private float GetNextMaxScore ()
	{
		//Gets the highest total score, including the current objective in the score. (to correctly scale bars)
		float highest = 1.0f;
		for ( int i = 0; i < 4; i++ )
		{
			if ( totalScores[i] + currentObjectiveScores[i] > highest )
			{
				highest = totalScores[i] + currentObjectiveScores[i];
			}
		}
		return highest;
	}

	private float GetMaxFinalScore()
	{
		//Gets the highest final score. (adjusted so going above highest total temporarily isn't a problem)
		float highest = 1.0f;
		for ( int i = 0; i < 4; i++ )
		{
			float sum = 0.0f;
			for ( int j = 0; j < NUMBER_OF_OBJECTIVES; j++) { sum += Mathf.Max ( 0.0f, GetObjectiveScoreFromIndex(i)[j].score ); }
			if ( sum > highest )
			{
				highest = sum;
			}
		}
		return highest;
	}

	private void AddObjectiveScoresToTotals ()
	{
		//adds current objective scores to the total score.
		for ( int i = 0; i < 4; i++ )
		{
			totalScores[i] += currentObjectiveScores[i];
		}
	}

	private void GetNextObjectiveScores()
	{
		//Load up the next wave of objective scores.
		for ( int i = 0; i < 4; i++ )
		{
			currentObjectiveScores[i] = GetObjectiveScoreFromIndex( i )[ objectiveScoreIndex ].score;
			nextTotalScores[i] = totalScores[i] + currentObjectiveScores[i];
		}
		objectiveScoreIndex++;
	}

	private SubScore[] GetObjectiveScoreFromIndex( int i )
	{
		//Turn a player's index into that player's objective score array.
		//index -> corresponding array (makes 4 distinctly named arrays behave like a 2D array)
		if      ( i == 0 ) { return objectiveScoresP1; }
		else if ( i == 1 ) { return objectiveScoresP2; }
		else if ( i == 2 ) { return objectiveScoresP3; }
		else if ( i == 3 ) { return objectiveScoresP4; }
		return null;
	}

	//Important functions
	private void DoStateStuff()
	{
		//---------------------------
		//Main cycle:
		//Get max score from current totals + next
		//Lerp between current score and next score (1s?). Blur OBJ. numbers.
		//Subtract obj. numbers from totals. (give sec to process)
		//repeat.
		//---------------------------
		if ( done ) { return; }

		stateTimer += Time.deltaTime;
		
		if ( stateTimer >= 1.0f )
		{
			stateTimer = 0.0f; //reset timer
			if ( state == 0 )  //on initialization
			{
				if ( objectiveScoreIndex < NUMBER_OF_OBJECTIVES )
				{
					//NOTE: I changed this to be based on the maximum final score
					//so that losing points made you go down, and getting points made you go up. (intuitive, no?)
					//When it was relative, gaining points slower than the leader could make you go down.
					state = 1;
					objectiveText.text = objectiveScoresP1[ objectiveScoreIndex ].name;
					GetNextObjectiveScores();
					//float max = GetMaxScore ();
					//float nextMax = GetNextMaxScore();
					float absMax = GetMaxFinalScore();
					for ( int i = 0; i < 4; i++ )
					{
						currentBarPercent[i] = totalScores[i] / absMax; //max;
						nextBarPercent[i] = ( totalScores[i] + currentObjectiveScores[i] ) / absMax; //nextMax;
						//Set text.
						totalScoreText[i].GetComponent<TextMesh>().text = ( (int)totalScores[i] ).ToString();
						objScoreText[i].GetComponent<TextMesh>().text = ( (int) currentObjectiveScores[i] ).ToString();
						if ( currentObjectiveScores[i] >= 0 )
						{
							objScoreText[i].GetComponent<TextMesh>().text = "+" + objScoreText[i].GetComponent<TextMesh>().text;
							objScoreText[i].GetComponent<TextMesh>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f );
						}
						else
						{
							objScoreText[i].GetComponent<TextMesh>().text = objScoreText[i].GetComponent<TextMesh>().text;
							objScoreText[i].GetComponent<TextMesh>().color = new Color( 0.9f, 0.0f, 0.0f, 1.0f );
						}
					}
				}
				else
				{
					//DONE. (all scores tallied)
					done = true;
					t = 0.0f; //reset timer.
					objectiveText.GetComponent<TextMesh>().text = "";
					for ( int i = 0; i < confettiEmitters.Length; i++ )
					{
						confettiEmitters[i].GetComponent<ParticleSystem>().Emit( 100 );
					}
					float max = 0.0f;
					for ( int i = 0; i < 4; i++ )
					{
						if ( totalScores[i] > max )
						{
							max = totalScores[i];
							//clear any previous winners
							for ( int j = 0; j < 4; j++ )
							{
								winner[j] = false;
							}
							winner[i] = true;
						}
						else if ( totalScores[i] == max )
						{
							//TIE!
							winner[i] = true;
						}
					}
				}
			}
			else if ( state == 1 ) //on completion
			{
				state = 0;
				AddObjectiveScoresToTotals (); 
				for ( int i = 0; i < 4; i++ )
				{
					totalScoreText[i].GetComponent<TextMesh>().text = ( (int)totalScores[i] ).ToString();
					objScoreText[i].GetComponent<TextMesh>().text = "";
				}
			}
		}

		//Lerp text
		if ( state == 1 )
		{
			for ( int i = 0; i < 4; i++ )
			{
				int j = (int) Mathf.Lerp ( totalScores[i], totalScores[i] + currentObjectiveScores[i], stateTimer );
				totalScoreText[i].GetComponent<TextMesh>().text = j.ToString();
			}
		}
	}

	private void MoveScoreBars()
	{
		//move 4 scorebars (-13.0f -> 0.0f)
		if ( state == 1 )
		{
			for ( int i = 0; i < 4; i++ )
			{
				float y = Mathf.Lerp ( -12.0f + 12.0f * currentBarPercent[i], -12.0f + 12.0f * nextBarPercent[i], Mathf.Min( 1.0f, stateTimer ) );
				scorebars[i].transform.position = new Vector3( scorebars[i].transform.position.x, y, 0.0f );
			}
		}
	}

	private void InitializeScoreBars()
	{
		for ( int i = 0; i < 4; i++ )
		{
			float y = -15.0f;
			scorebars[i].transform.position = new Vector3( scorebars[i].transform.position.x, y, 0.0f );
			//set up sprites
			if ( charClasses[i] == CharacterClasses.KNIGHT )
			{
				playerSprites[i].GetComponent<Animator>().runtimeAnimatorController = knightAnis[i];
			}
			else if ( charClasses[i] == CharacterClasses.DEFENDER )
			{
				playerSprites[i].GetComponent<Animator>().runtimeAnimatorController = monkAnis[i];
			}
			playerSprites[i].GetComponent<Animator>().Play ( "idle_down" );
		}
	}

	private void MakeWinnersJump ()
	{
		//Jump
		float jump = Mathf.Abs ( 0.5f * Mathf.Sin ( 3.0f * t ) ) -  Mathf.Abs ( 0.5f * Mathf.Sin ( 3.0f * (t - Time.deltaTime) ) );
		for ( int i = 0; i < 4; i++ )
		{
			if ( winner[i] )
			{
				playerSprites[i].GetComponent<Animator>().Play ( "pickup_down" );
				playerSprites[i].transform.position = playerSprites[i].transform.position + new Vector3( 0.0f, jump, 0.0f);
				spotLights[i].GetComponent<SpriteRenderer>().enabled = true;
			}
		}
	}
}
