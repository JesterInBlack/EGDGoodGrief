using UnityEngine;
using System.Collections;

public class ExplodingSacProjectileScript : MonoBehaviour 
{
	public GameObject _sacShot;
	public GameObject _targetPlayer;

	private Vector2 _startPos;
	private Vector2 _playerPos;
	private Vector2 _directionVector;

	public float _flightDuration = 0.3f;
	public float _lerpTime = 0.0f;

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if(Vector3.Distance(_playerPos, transform.position) < 0.001f)
		{
			_lerpTime = 0.0f;
			_targetPlayer.GetComponent<Player>().KnockBack(0.0f, Vector2.zero);

			//turns on the head grabber
			_targetPlayer.GetComponent<Player>().headGrabber.GrabHead ();
			Destroy(this.gameObject);
		}
		else
		{
			_playerPos = _targetPlayer.transform.position;
			float xDiff = _playerPos.x - transform.position.x; 
			float yDiff = _playerPos.y - transform.position.y; 
			float rotationAngle = Mathf.Atan2(yDiff, xDiff) * (180.0f / Mathf.PI);
			transform.eulerAngles = new Vector3(0, 0, rotationAngle);
			
			transform.position = Vector2.Lerp(_startPos, _playerPos, _lerpTime / _flightDuration);
			_lerpTime += (Time.deltaTime* StaticData.t_scale);
		}
	}

	public void Initializer(Vector2 startPos, GameObject targetPlayer)
	{
		_targetPlayer = targetPlayer;
		transform.position = startPos;
		_startPos = transform.position;
		_playerPos = targetPlayer.transform.position;
		float xDiff = _playerPos.x - startPos.x; 
		float yDiff = _playerPos.y - startPos.y; 
		float rotationAngle = Mathf.Atan2(yDiff, xDiff) * (180.0f / Mathf.PI);
		
		transform.eulerAngles = new Vector3(0, 0, rotationAngle);
		
		//calcualte the end point of the tether
		_directionVector = new Vector2(xDiff, yDiff);
		_directionVector.Normalize();
	}
}
