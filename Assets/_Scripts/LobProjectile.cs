using UnityEngine;
using System.Collections;

public class LobProjectile : MonoBehaviour {

	public GameObject _blast;
	public GameObject _projectile;
	public GameObject _shadow;

	public Vector2 _shadowStartPosition;
	public Vector2 _projectileStartPosition;
	public Vector2 _controlPoint;

	public Vector2 _targetPosition;

	public bool _readyToStart = false;
	private float _totalTime = 2.0f;
	public float _currentTime;

	// Use this for initialization
	void Start () 
	{
		_currentTime = 0.0f;

	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_readyToStart == true)
		{
			if(_currentTime < _totalTime)
			{
				_projectile.transform.position = CalculateBezierPoint(_currentTime/_totalTime, (Vector3)_projectileStartPosition, (Vector3)_targetPosition, (Vector3)_controlPoint);
				_shadow.transform.position = Vector3.Lerp((Vector3)_shadowStartPosition, (Vector3)_targetPosition, _currentTime/_totalTime);
			}
			else
			{
				Instantiate(_blast, _targetPosition, Quaternion.identity);
				Destroy(this.gameObject);
			}
			_currentTime += (Time.deltaTime * StaticData.t_scale);
		}
	}

	private Vector3 CalculateBezierPoint(float t, Vector3 startPos, Vector3 endPos, Vector3 controlPoint)
	{
		float u = 1 - t;
		float uu = u * u;
		
		Vector3 point = uu * startPos;
		point += 2 * u * t * controlPoint;
		point += t * t * endPos;
		
		return point;
	}

	public void Initializer(Vector2 projectileStartPos, Vector2 shadowStartPos, Vector2 targetPos)
	{
		_projectileStartPosition = projectileStartPos;
		_shadowStartPosition = shadowStartPos;
		_targetPosition = targetPos;

		_controlPoint = GetIntermediatePoint(_projectileStartPosition, _targetPosition);
		_readyToStart = true;
	}

	//returns the midpoint of ab and sets it up a bit
	Vector2 GetIntermediatePoint(Vector2 a, Vector2 b)
	{
		float xPoint = a.x + ((b.x - a.x)/2);
		float yPoint = a.y + ((b.y - a.y)/2);
		
		yPoint += ((b.y - a.y)/2);

		return new Vector2(xPoint, yPoint);
	}
}
