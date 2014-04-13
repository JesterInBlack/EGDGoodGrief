using UnityEngine;
using System.Collections;

public class PointLaserScript : MonoBehaviour 
{
	public GameObject _pointExplosion;

	private Vector2 _startPos;
	private Vector2 _endPos;
	//private float _rotationAngle;

	public float _flightDuration = 0.3f;
	public float _lerpTime = 0.0f;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Vector3.Distance(_endPos, transform.position) < 0.001f)
		{
			Instantiate(_pointExplosion, transform.position, Quaternion.identity);
			Destroy(this.gameObject);
		}
		else
		{
			transform.position = Vector2.Lerp(_startPos, _endPos, _lerpTime / _flightDuration);
			_lerpTime += (Time.deltaTime* StaticData.t_scale);
		}
	}

	public void Initializer(Vector2 startPos, Vector2 endPos)
	{
		transform.position = startPos;
		_startPos = transform.position;
		_endPos = endPos;
		float xDiff = endPos.x - startPos.x; 
		float yDiff = endPos.y - startPos.y; 
		float rotationAngle = Mathf.Atan2(yDiff, xDiff) * (180.0f / Mathf.PI);

		transform.eulerAngles = new Vector3(0, 0, rotationAngle);
	}
}
