using UnityEngine;
using System.Collections;

public class EyeLaserScript : MonoBehaviour 
{
	private const float _degenRate = 5.0f;       

	private Vector3 _startPos;
	//private float _rotationAngle;
	public float _travelDistance = 20.0f;
	public float _speed = 3.5f;
	public float _hitRadius;

	// Use this for initialization
	void Start () 
	{
		_hitRadius = GetComponent<CircleCollider2D>().radius;
	}
	
	// Update is called once per frame
	void Update () 
	{
		//AttackSystem.hitCircle( transform.position, 1.0f, 1.0f * Time.deltaTime * StaticData.t_scale, -1 );
		if(Vector3.Distance(_startPos, transform.position) > _travelDistance)
		{
			Destroy(this.gameObject);
		}
		else
		{
			transform.position += transform.right * _speed * Time.deltaTime * StaticData.t_scale;
		}
		/*
		foreach ( Collider2D hit in AttackSystem.getHitsInCircle( (Vector2)transform.position, _hitRadius, -1) )
		{
			Player tempPlayer = hit.collider.gameObject.GetComponent<Player>();
			if ( tempPlayer != null ) //this is a player.
			{
				if ( ! tempPlayer.isCarried ) //if the player is not being carried
				{
					if ( ! tempPlayer.isDowned )
					{
						float dt = Time.deltaTime;
						//Degenerate health!
						if ( ! tempPlayer.isInBulletTime ) { dt = dt * StaticData.t_scale; }
						tempPlayer.HP = Mathf.Max ( player.HP - (_degenRate * dt / tempPlayer.defense), 0.0f );
					}
				}
			}
		}
		*/

		for ( int i = 0; i < GameState.players.Length; i++ )
		{
			Player player = GameState.players[i].GetComponent<Player>();
			//get distance from player's center to the hit circle's center
			BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
			Vector3 theirPos = player.gameObject.transform.position;
			CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
			Vector3 myPos = this.gameObject.transform.position;
			float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
			float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
			float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );
			
			if ( dist <= circle.radius ) //if player is in the region (collider center point in radius)
			{
				if ( ! player.isCarried ) //if the player is not being carried
				{
					if ( ! player.isDowned )
					{
						float dt = Time.deltaTime;
						//Degenerate health!
						if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; }
						player.HP = Mathf.Max ( player.HP - (_degenRate * dt / player.defense), 0.0f );
					}
				}
			}
		}

	}

	public void Initializer(Vector3 startPos, float rotationAngle)
	{
		transform.position = startPos;
		_startPos = transform.position;
		transform.eulerAngles = new Vector3(0, 0, rotationAngle);
	}
}
