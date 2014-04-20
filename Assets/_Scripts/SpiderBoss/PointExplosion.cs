using UnityEngine;
using System.Collections;

public class PointExplosion : MonoBehaviour 
{
	public float _damageValue = 15.0f;
	public float _chargeDuration = 1.5f;
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
