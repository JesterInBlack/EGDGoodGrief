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
	private float _cancelDuration;

	private Vector3 _baseSize;
	private GameObject _spawnPoint;

	public GameObject whiteoutPrefab; //set in inspector.
	public GameObject overlayPrefab;  //set in inspector.

	private enum State
	{
		Charging = 0,
		Fire = 1,
		Cancel = 2,
	}
	private State _state;

	// Use this for initialization
	void Start () 
	{
		transform.localScale = Vector3.zero;
		_baseSize = new Vector3(1, 1, 1);

		_lerpTime = 0.0f;
		_cancelDuration = 0.2f;
		_state = State.Charging;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_state == State.Charging)
		{
			transform.position = _spawnPoint.transform.position;
			transform.localScale = Vector3.Lerp(Vector3.zero, _baseSize, _lerpTime / _chargeDuration);
			_lerpTime += (Time.deltaTime * StaticData.t_scale);
			_startPos = transform.position;

			//Vibration
			for ( int i = 0; i < 4; i++ )
			{
				GameState.players[i].GetComponent<VibrationManager>().AddVibrationForThisFrame ( 
					_lerpTime / _chargeDuration * 0.5f, _lerpTime / _chargeDuration * 0.5f );
			}
		}
		else if(_state == State.Fire)
		{
			if ( Vector3.Distance(_endPos, transform.position) < 0.001f )
			{
				//AttackSystem.hitCircle((Vector2)transform.position, 1000.0f, 75.0f, -1); //slightly more efficient my way.
				for ( int i = 0; i < GameState.players.Length; i++ )
				{
					GameState.playerStates[i].Hurt ( 40.0f );
				}
				GameState.cameraController.Shake (0.4f, 2.0f );
				Instantiate ( whiteoutPrefab, transform.position, Quaternion.identity );
				Destroy ( this.gameObject );
			}
			else
			{
				transform.position = Vector2.Lerp(_startPos, _endPos, _lerpTime / _travelTime);
				_lerpTime += (Time.deltaTime * StaticData.t_scale);
			}
		}
		else if(_state == State.Cancel)
		{
			if(transform.localScale == Vector3.zero)
			{
				Destroy(this.gameObject);
			}
			else
			{
				transform.position = _spawnPoint.transform.position;
				transform.localScale = Vector3.Lerp(_baseSize, Vector3.zero, _lerpTime / _cancelDuration);
				_lerpTime += (Time.deltaTime * StaticData.t_scale);
			}
		}
	}

	public void Initializer(GameObject parent, Vector3 startPos, Vector3 endPos, float chargeDuration)
	{
		_spawnPoint = parent;
		transform.position = startPos;
		_startPos = transform.position;
		_endPos = endPos;
		_chargeDuration = chargeDuration;
	}
	public void Attack()
	{
		_lerpTime = 0.0f;
		_state = State.Fire;
	}
	public void Cancel()
	{
		_lerpTime = 0.0f;
		_state = State.Cancel;
		_baseSize = transform.localScale;
	}
}
