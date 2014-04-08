using UnityEngine;
using System.Collections;

public class MegaFlareScript : MonoBehaviour 
{
	private Vector3 _startPos;
	private Vector3 _endPos;
	//private float _rotationAngle;
	public float _lerpTime = 0.0f;
	public float _travelTime = 1.5f;

	private float _chargeDuration;
	private bool _charging;

	private Vector3 _baseSize;

	// Use this for initialization
	void Start () 
	{
		transform.localScale = Vector3.zero;
		_baseSize = new Vector3(1, 1, 1);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_charging)
		{
			transform.localScale = Vector3.Lerp(Vector3.zero, _baseSize, _lerpTime / _chargeDuration);
			_lerpTime += (Time.deltaTime * StaticData.t_scale);
		}
		else
		{
			if(Vector3.Distance(_endPos, transform.position) < 0.001f)
			{
				//TODO Make this damage people
				Destroy(this.gameObject);
			}
			else
			{
				transform.position = Vector2.Lerp(_startPos, _endPos, _lerpTime / _travelTime);
				_lerpTime += (Time.deltaTime * StaticData.t_scale);
			}
		}
	}

	public void Initializer(Vector3 startPos, Vector3 endPos, float chargeDuration)
	{
		transform.position = startPos;
		_startPos = transform.position;
		_endPos = endPos;
		_charging = true;
		_chargeDuration = chargeDuration;
	}
	public void Attack()
	{
		_lerpTime = 0.0f;
		_charging = false;
	}
}
