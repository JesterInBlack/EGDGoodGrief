using UnityEngine;
using System.Collections;

public class PointExplosion : MonoBehaviour 
{
	private const float _degenRate = 1.5f;

	public float _damageValue = 15.0f;
	public float _chargeDuration = 1.75f;
	public float _chargeTime;
	public GameObject lavaPrefab;

	private float _damageRadius;
	
	public GameObject _pointField;
	private float _alphaValue;
	private SpriteRenderer _spriteRenderer;

	// Use this for initialization
	void Start () 
	{
		_damageRadius = GetComponent<CircleCollider2D>().radius;
		_spriteRenderer = _pointField.GetComponent<SpriteRenderer>();
		_alphaValue = 0.0f;
		_spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, _alphaValue);

		_chargeTime = 0.0f;

	}
	
	// Update is called once per frame
	void Update ()
	{
		if(_chargeTime < _chargeDuration)
		{
			_chargeTime += Time.deltaTime * StaticData.t_scale;
			_alphaValue = Mathf.Pow(_chargeTime/_chargeDuration, 2);

			#region damage players
			float dt = Time.deltaTime * StaticData.t_scale;
			for ( int i = 0; i < GameState.players.Length; i++ )
			{
				if ( GameState.players[i] != null )
				{
					Player player = GameState.playerStates[i];
					//get distance from player's center to the hit circle's center
					BoxCollider2D box = player.gameObject.GetComponent<BoxCollider2D>();
					Vector3 theirPos = player.gameObject.transform.position;
					CircleCollider2D circle = this.gameObject.GetComponent<CircleCollider2D>();
					Vector3 myPos = this.gameObject.transform.position;
					float xdist = ( (myPos.x + circle.center.x) - (theirPos.x + box.center.x) );
					float ydist = ( (myPos.y + circle.center.y) - (theirPos.y + box.center.y) );
					float dist = Mathf.Pow(  xdist * xdist + ydist * ydist, 0.5f );
					
					if ( dist <= circle.radius * transform.localScale.x ) //if player is in the region (collider center point in radius)
					{
						if ( ! player.isCarried ) //if the player is not being carried
						{
							if ( ! player.isDowned )
							{
								//Degenerate health!
								if ( ! player.isInBulletTime ) { dt = dt * StaticData.t_scale; }
								player.HP = Mathf.Max ( player.HP - (_degenRate * dt / player.defense), 0.0f );
							}
						}
					}
				}
			}
			#endregion
		}
		else
		{
			AttackSystem.hitCircle (transform.position, _damageRadius * transform.lossyScale.x, _damageValue, -1 );
			GameState.cameraController.Shake (0.1f, 0.25f );
			Instantiate ( lavaPrefab, this.gameObject.transform.position, Quaternion.identity );
			Destroy(this.gameObject);
		}
		_spriteRenderer.color = new Color(1.0f, 0.0f, 0.0f, _alphaValue);
	}
}
