using UnityEngine;
using System.Collections;

public class DamageNumbers : MonoBehaviour 
{
	//TODO: make this script get called by scoremanager (+ onhitcallback?)
	//TODO: make this script call players show text for defensive. For offensive, place own text at location.
	//TODO: link to player.
	//TODO: record player taking / dealing / blocking damage over time. (3 separate timers) (reset on new damage)
	//TODO: improvement: 1 timer per attacker id (4 + 1, track independently)
	//TODO: if the timer runs out, put a damage number up on the screen.

	//TODO: color code + points to characters???

	#region vars
	private const float takeDelay = 0.1f;   //how long, in seconds, needs to pass before an attack is considered separate.
	private const float dealDelay = 0.1f;   //how long, in seconds, needs to pass before an attack is considered separate.
	private const float blockDelay = 0.1f;  //how long, in seconds, needs to pass before an attack is considered separate.

	private float[] blockTimer = new float[5];
	private float[] takeTimer  = new float[5];
	private float dealTimer;
	
	private float[] blockPoints = new float[5];
	private float[] takePoints  = new float[5];
	private float dealPoints;

	private Player player;
	#endregion

	void Awake()
	{
		player = GetComponent<Player>();
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		//damage blocked + points
		for ( int i = 0; i < blockTimer.Length; i++ )
		{
			float prevVal = blockTimer[i];
			blockTimer[i] -= Time.deltaTime;
			if ( prevVal > 0.0f && blockTimer[i] <= 0.0f )
			{
				ScoreText( "Block", blockPoints[i] );
				blockPoints[i] = 0.0f;
			}
		}

		//damage taken - points
		for ( int i = 0; i < takeTimer.Length; i++ )
		{
			float prevVal = takeTimer[i];
			takeTimer[i] -= Time.deltaTime;
			if ( prevVal > 0.0f && takeTimer[i] <= 0.0f )
			{
				ScoreText( "", takePoints[i] );
				takePoints[i] = 0.0f;
			}
		}

		//Damage dealt + points
		float prevVal2 = dealTimer;
		dealTimer -= Time.deltaTime;
		if ( prevVal2 > 0.0f && dealTimer <= 0.0f )
		{
			ScoreText( "", dealPoints );
			dealPoints = 0.0f;
		}
	}

	public void AddBlockPoints ( int attackerId, float points )
	{
		//handles the + points on your side
		if ( attackerId == -1 ) { attackerId = 4; }
		blockTimer[attackerId] = blockDelay;
		blockPoints[attackerId] += points;
	}

	public void AddDealDamagePoints( float points )
	{
		//handles the + points on your side
		dealTimer = dealDelay;
		dealPoints += points;
	}

	public void AddTakeDamagePoints( int attackerId, float points )
	{
		//handles - points on your side.
		//Note leg and boss core will need a similar implementation to this.
		if ( attackerId == -1 ) { attackerId = 4; }
		takeTimer[attackerId] = takeDelay;
		takePoints[attackerId] -= points;
	}

	private void ScoreText ( string name, float amount )
	{
		GameObject obj = (GameObject)Instantiate( player.scoreTextPrefab, transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), Quaternion.identity );
		obj.GetComponent<ScoreText>().scoreName.text = name;
		float scale = 0.80f;
		obj.transform.localScale = new Vector3( scale, scale, scale );
		obj.transform.parent = this.gameObject.transform;
		
		string text;
		Color color;
		if ( amount > 0.0f ) 
		{ 
			text = "+" + ((int) amount); 
			color = /*GetPlayerColor ();*/ new Color( 0.0f, 0.0f, 1.0f, 1.0f );
		}
		else 
		{ 
			text = ((int) amount).ToString();
			color = new Color( 0.9f, 0.0f, 0.0f, 1.0f );
		}
		obj.GetComponent<ScoreText>().scorePoints.text = text;
		obj.GetComponent<ScoreText>().scorePoints.color = color;
	}

	private Color GetPlayerColor()
	{
		if ( player.id == 0 )
		{
			return new Color( 0.9f, 0.0f, 0.0f, 1.0f );
		}
		else if ( player.id == 1 )
		{
			return new Color( 0.0f, 0.0f, 1.0f, 1.0f );
		}
		else if ( player.id == 2 )
		{
			return new Color( 0.0f, 0.9f, 0.0f, 1.0f );
		}
		else if ( player.id == 3 )
		{
			return new Color( 1.0f, 0.9f, 0.0f, 1.0f );
		}
		return Color.black;
	}
}
