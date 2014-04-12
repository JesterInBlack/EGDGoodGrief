using UnityEngine;
using System.Collections;

public class PostGameScore : MonoBehaviour 
{
	#region vars
	public GameObject[] scorebars = new GameObject[4];     //set in inspector
	public GameObject[] playerSprites = new GameObject[4]; //set in inspector (need to set sprite renderer, + animate it, + jump, + move with scorebar)
	float t;
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
}
