using UnityEngine;
using System.Collections;

public class EyeLaserScript : MonoBehaviour 
{
	private const float _degenRate = 8.0f;       

	private Vector3 _startPos;
	//private float _rotationAngle;
	public float _travelDistance = 25.0f;
	public float _speed = 5f;
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
			float percentDist = Vector3.Distance(_startPos, transform.position) / _travelDistance;
			//scale so only the last 20% blends out.
			//float lerpT = 0.0f;
			GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, 1.0f - percentDist );
		}

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

			float boxRadius = (Mathf.Min(box.size.y, box.size.x) / 2.0f); //approximation of hitbox by inscribed circle

			if ( dist <= circle.radius + boxRadius ) //if player is in the region (collider center point in radius)
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
