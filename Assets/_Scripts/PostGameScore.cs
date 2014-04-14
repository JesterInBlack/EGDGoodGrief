﻿using UnityEngine;
using System.Collections;

public class PostGameScore : MonoBehaviour 
{
	#region vars
	public GameObject[] scorebars = new GameObject[4];              //set in inspector
	public GameObject[] playerSprites = new GameObject[4];          //set in inspector (need to set sprite renderer, + animate it, + jump, + move with scorebar)
	public ScoreDetails[] playerScoreDetails = new ScoreDetails[4]; //pulled from objects?
	public GameObject[] confettiEmitters = new GameObject[5];       //set in inspector

	private float[] currentBarPercent = new float[4];
	private float[] nextBarPercent = new float[4];

	private float[] totalScores = new float[4];
	private float[] currentObjectiveScores = new float[4];
	private float[] nextTotalScores = new float[4];

	//2D arrays don't want to be jagged, ergo this badness.
	//TODO: change to subscore
	private SubScore[] objectiveScoresP1 = new SubScore[5];
	private SubScore[] objectiveScoresP2 = new SubScore[5];
	private SubScore[] objectiveScoresP3 = new SubScore[5];
	private SubScore[] objectiveScoresP4 = new SubScore[5];

	private int objectiveScoreIndex = 0;
	private const int NUMBER_OF_OBJECTIVES = 5; //# of subscores that go into the total score.
	private float t;                            //periodic timer for jumping animation
	private float stateTimer = 0.0f;            //timer for state transitions
	private int state = 0;                      //state tracker
	private bool done = false;                  //if the state loop is done
	private bool[] winner = {false, false, false, false};               
	#endregion
	//TODO: add text: total numbers under them, subtotals for each round.
	//TODO: add "Winner" confetti + effects

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
			totalScores[i] = 0.0f;
			currentObjectiveScores[i] = 0.0f;
			currentBarPercent[i] = 0.0f;
		}

		#region FakeData
		objectiveScoresP1[0].score = 10;
		objectiveScoresP2[0].score = 5;
		objectiveScoresP3[0].score = 7;
		objectiveScoresP4[0].score = 3;

		objectiveScoresP1[1].score = 0;
		objectiveScoresP2[1].score = -2;
		objectiveScoresP3[1].score = 0;
		objectiveScoresP4[1].score = -1;

		objectiveScoresP1[2].score = 1;
		objectiveScoresP2[2].score = 2;
		objectiveScoresP3[2].score = 3;
		objectiveScoresP4[2].score = 4;

		objectiveScoresP1[3].score = -1;
		objectiveScoresP2[3].score = -1;
		objectiveScoresP3[3].score = -1;
		objectiveScoresP4[3].score = -1;

		objectiveScoresP1[4].score = 0;
		objectiveScoresP2[4].score = 0;
		objectiveScoresP3[4].score = 10;
		objectiveScoresP4[4].score = 0;
		#endregion
	}

	// Use this for initialization
	void Start () 
	{
		scorebars[0].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.0f, 0.0f, 0.5f );
		scorebars[1].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 0.0f, 1.0f, 0.5f );
		scorebars[2].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 1.0f, 0.0f, 0.5f );
		scorebars[3].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 0.0f, 0.5f );
		InitializeScoreBars (); //initialize them.
	}
	
	// Update is called once per frame
	void Update () 
	{
		t += Time.deltaTime;
		DoStateStuff();
		MoveScoreBars();
		MakeWinnersJump();
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
		//Gets the highest final score.
		float highest = 1.0f;
		for ( int i = 0; i < 4; i++ )
		{
			float sum = 0.0f;
			for ( int j = 0; j < NUMBER_OF_OBJECTIVES; j++) { sum += GetObjectiveScoreFromIndex(i)[j].score;}
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
					GetNextObjectiveScores();
					//float max = GetMaxScore ();
					//float nextMax = GetNextMaxScore();
					float absMax = GetMaxFinalScore();
					for ( int i = 0; i < 4; i++ )
					{
						currentBarPercent[i] = totalScores[i] / absMax; //max;
						nextBarPercent[i] = ( totalScores[i] + currentObjectiveScores[i] ) / absMax; //nextMax;
					}
				}
				else
				{
					//DONE. (all scores tallied)
					done = true;
					t = 0.0f; //reset timer.
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
			float y = -13.0f;
			scorebars[i].transform.position = new Vector3( scorebars[i].transform.position.x, y, 0.0f );
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
			}
		}
	}
}
