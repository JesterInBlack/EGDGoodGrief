using UnityEngine;
using System.Collections;

public class HeadGrabber : MonoBehaviour 
{
	private Player player;

	public bool on = false;                       //are you infected?
	public float timeLeft = 0.0f;                 //time left until it explodes
	public float nonTransferrableTimeLeft = 0.0f; //time left until it can be transferred again.
	private const float transferCooldown = 1.0f;  //wait time between transfers
	private const float burstDamage = 30.0f;      //amount of damage dealt when it expires
	private const bool burstExplodes = false;     //whether or not the burst explodes, dealing AoE rather than single target damage.
	private const float dotDamage = 1.0f;         //DPS while you're infected.

	//Use this for pre-initialization
	void Awake ()
	{
		player = this.gameObject.transform.parent.GetComponent<Player>();
		player.headGrabber = this;
	}

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( ! on ) 
		{ 
			GetComponent<SpriteRenderer>().enabled = false;
			return; 
		} 
		GetComponent<SpriteRenderer>().enabled = true;

		float dt = Time.deltaTime;
		if ( player.isInBulletTime == false ) { dt = dt * StaticData.t_scale; }

		//DoT tick.
		player.HP = Mathf.Max ( 1.0f, ( player.HP - dotDamage * dt ) / player.defense );

		timeLeft = Mathf.Max ( 0.0f, timeLeft - dt );
		nonTransferrableTimeLeft = Mathf.Max ( 0.0f, nonTransferrableTimeLeft - dt );
		if ( timeLeft <= 0.0f )
		{
			//YOUR HEAD ASLPODE!
			player.Hurt ( burstDamage );
			on = false;
		}
		else if ( nonTransferrableTimeLeft <= 0.0f )
		{
			//check if you're touching another player! (box - box)
			int layerMask = 1 << LayerMask.NameToLayer ( "Player" );
			BoxCollider2D box = this.gameObject.transform.parent.GetComponent<BoxCollider2D>();
			float x = box.transform.position.x + box.center.x;
			float y = box.transform.position.y + box.center.y;
			foreach ( Collider2D hit in Physics2D.OverlapAreaAll( 
				new Vector2 ( x, y ),
			    new Vector2 ( x + box.size.x, y + box.size.y ),
			    layerMask) )
			{
				if ( hit.gameObject.GetComponent<Player>() != null )
				{
					if ( hit.gameObject.GetComponent<Player>().id != player.id ) //not self
					{
						//their on = true, copy stats over.
						HeadGrabber theirs = hit.gameObject.GetComponent<Player>().headGrabber;
						theirs.on = true;
						theirs.nonTransferrableTimeLeft = transferCooldown;
						theirs.timeLeft = timeLeft;

						on = false;
						return; //force break.
					}
				}
			}
		}
	}
}
