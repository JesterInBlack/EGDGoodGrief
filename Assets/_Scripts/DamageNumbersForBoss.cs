using UnityEngine;
using System.Collections;

public class DamageNumbersForBoss : MonoBehaviour 
{		
	#region vars
	private const float takeDelay = 0.1f;   //how long, in seconds, needs to pass before an attack is considered separate.
	private float[] takeTimer  = new float[5];
	private float[] takePoints  = new float[5];
	#endregion
	
	// Update is called once per frame
	void Update () 
	{
		//damage taken - points
		for ( int i = 0; i < takeTimer.Length; i++ )
		{
			float prevVal = takeTimer[i];
			takeTimer[i] -= Time.deltaTime;
			if ( prevVal > 0.0f && takeTimer[i] <= 0.0f )
			{
				ScoreText( "", takePoints[i], i );
				takePoints[i] = 0.0f;
			}
		}
	}
	
	public void AddTakeDamagePoints( int attackerId, float points )
	{
		//handles - points on your side.
		//Note leg and boss core will need a similar implementation to this.
		if ( attackerId == -1 ) { attackerId = 4; }
		takeTimer[attackerId] = takeDelay;
		takePoints[attackerId] += points;
	}
	
	private void ScoreText ( string name, float amount, int id )
	{
		GameObject obj = (GameObject)Instantiate( GameState.playerStates[id].scoreTextPrefab, transform.position + new Vector3( 0.0f, 1.0f, 0.0f ), Quaternion.identity );
		obj.GetComponent<ScoreText>().scoreName.text = name;
		float scale = 0.85f;
		obj.transform.localScale = new Vector3( scale, scale, scale );
		obj.transform.parent = this.gameObject.transform;
	
		string text = ((int) amount).ToString(); 
		Color color = GetPlayerColor ( id );
		obj.GetComponent<ScoreText>().scorePoints.text = text;
		obj.GetComponent<ScoreText>().scorePoints.color = color;
	}
	
	private Color GetPlayerColor( int id )
	{
		if ( id == 0 )
		{
			return new Color( 0.9f, 0.0f, 0.0f, 1.0f );
		}
		else if ( id == 1 )
		{
			return new Color( 0.0f, 0.0f, 1.0f, 1.0f );
		}
		else if ( id == 2 )
		{
			return new Color( 0.0f, 0.9f, 0.0f, 1.0f );
		}
		else if ( id == 3 )
		{
			return new Color( 1.0f, 0.9f, 0.0f, 1.0f );
		}
		return Color.black;
	}
}
