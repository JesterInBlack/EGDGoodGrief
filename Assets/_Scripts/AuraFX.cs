using UnityEngine;
using System.Collections;

public class AuraFX : MonoBehaviour 
{

	#region vars
	public float rotationSpeed = 90.0f; //degrees per second.
	private float rotationSpeed2;       //speed with variance

	public enum AuraType { OFFENSE, REGEN, DEFENSE };
	public AuraType auraType;          //set in inspector
	private Player player;             //player to rip aura buff status from
	private bool prevOn = false;       //previous frame tracking for "turn on" effect
	private float lerpt = 0.0f;        //weighting to gradually remove the initial burst of speed.
	#endregion

	// Use this for initialization
	void Start () 
	{
		player = gameObject.transform.parent.GetComponent<Player>();
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//Go through buffs, check if the one that matches this aura is on. If it is, turn this aura on.
		bool on = false;
		for ( int i = 0; i < player.buffs.Count; i++ )
		{
			if ( ((( Buff ) player.buffs[i] ).defense > 0.0f && auraType == AuraType.DEFENSE) ||
			     ((( Buff ) player.buffs[i] ).offense > 0.0f && auraType == AuraType.OFFENSE) ||
			     ((( Buff ) player.buffs[i] ).regen   > 0.0f && auraType == AuraType.REGEN ) )
			{
				on = true;
			}
		}

		if ( on && ! prevOn ) //turned on
		{
			rotationSpeed2 = rotationSpeed * 2.0f; //start fast!
			lerpt = 0.0f;
			gameObject.GetComponent<SpriteRenderer>().enabled = true;
		}
		if ( ! on && prevOn ) //turned off
		{
			gameObject.GetComponent<SpriteRenderer>().enabled = false;
		}

		lerpt = Mathf.Min ( lerpt + Time.deltaTime, 1.0f );
		float omega = Mathf.Lerp ( rotationSpeed2, rotationSpeed, lerpt );
		transform.Rotate ( new Vector3 ( 0.0f, 0.0f, Time.deltaTime * omega ) );
		prevOn = on;
	}
}
