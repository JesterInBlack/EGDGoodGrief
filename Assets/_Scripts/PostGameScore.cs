using UnityEngine;
using System.Collections;

public class PostGameScore : MonoBehaviour 
{
	#region vars
	public GameObject[] scorebars = new GameObject[4];              //set in inspector
	public GameObject[] playerSprites = new GameObject[4];          //set in inspector (need to set sprite renderer, + animate it, + jump, + move with scorebar)
	public ScoreDetails[] playerScoreDetails = new ScoreDetails[4]; //pulled from objects?

	private float[] totalScores = new float[4];
	private float[] currentObjectiveScores = new float[4];
	private float[] nextTotalScores = new float[4];

	//2D arrays don't want to be jagged, ergo this badness.
	private float[] objectiveScoresP1 = new float[5];
	private float[] objectiveScoresP2 = new float[5];
	private float[] objectiveScoresP3 = new float[5];
	private float[] objectiveScoresP4 = new float[5];

	private int objectiveScoreIndex = 0;
	private float t;
	#endregion
	//TODO: add text: total numbers under them, subtotals for each round.
	//TODO: add "Winner" confetti + effects

	//Use this for pre-initialization
	void Awake ()
	{
		//pull data from passed score object.
		//pull SCORES
		//pull PLAYER COUNT (+ center scorebars appropriately)
		//pull PLAYER CLASS (+ set animators + sprites appropriately) (copy from pre-game)

		for ( int i = 0; i < 4; i++ )
		{
			totalScores[i] = 0.0f;
			currentObjectiveScores[i] = 0.0f;
		}
	}

	// Use this for initialization
	void Start () 
	{
		scorebars[0].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 0.0f, 0.0f, 0.5f );
		scorebars[1].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 0.0f, 1.0f, 0.5f );
		scorebars[2].GetComponent<SpriteRenderer>().color = new Color( 0.0f, 1.0f, 0.0f, 0.5f );
		scorebars[3].GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 0.0f, 0.5f );
	}
	
	// Update is called once per frame
	void Update () 
	{
		objectiveScoresP1 = playerScoreDetails[0].GetObjectiveScores();
		objectiveScoresP2 = playerScoreDetails[1].GetObjectiveScores();
		objectiveScoresP3 = playerScoreDetails[2].GetObjectiveScores();
		objectiveScoresP4 = playerScoreDetails[3].GetObjectiveScores();
		//---------------------------
		//Main cycle:
		//Get max score from current totals + next
		//Lerp between current score and next score (1s?). Blur OBJ. numbers.
		//Subtract obj. numbers from totals. (give sec to process)
		//repeat.
		//---------------------------



		//move 4 scorebars (-13.0f -> 0.0f)
		t += Time.deltaTime;
		//float y = Mathf.Lerp ( -13.0f, 0.0f, (t / 5.0f) % 1.0f );
		float y = 0.0f;
		scorebars[0].transform.position = new Vector3( scorebars[0].transform.position.x, y, 0.0f );

		//Jump
		float jump = Mathf.Abs ( 0.5f * Mathf.Sin ( 3.0f * t ) ) -  Mathf.Abs ( 0.5f * Mathf.Sin ( 3.0f * (t - Time.deltaTime) ) );
		playerSprites[0].GetComponent<Animator>().Play ( "pickup_down" );
		playerSprites[0].transform.position = playerSprites[0].transform.position + new Vector3( 0.0f, jump , 0.0f);
		//TODO: functionalize this per player, call x4
	}

	private float GetMaxScore ()
	{
		//Gets the highest total score. (to correctly scale bars)
		float highest = 0.0f;
		for ( int i = 0; i < 4; i++ )
		{
			if ( totalScores[i] > highest )
			{
				highest = totalScores[i];
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
			currentObjectiveScores[i] = GetObjectiveScoreFromIndex( i )[ objectiveScoreIndex ];
			nextTotalScores[i] = totalScores[i] + currentObjectiveScores[i];
		}
		objectiveScoreIndex++;
	}

	private float[] GetObjectiveScoreFromIndex( int i )
	{
		//Turn a player's index into that player's objective score array.
		if      ( i == 0 ) { return objectiveScoresP1; }
		else if ( i == 1 ) { return objectiveScoresP2; }
		else if ( i == 2 ) { return objectiveScoresP3; }
		else if ( i == 3 ) { return objectiveScoresP4; }
		return null;
	}
}
