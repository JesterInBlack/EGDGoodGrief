using UnityEngine;
using System.Collections;

public class WebBuffScript : MonoBehaviour 
{
	public float _alpha;
	public float _transitDuration;
	public float _transitTime;

	public enum State
	{
		Idle = 0,
		Apply = 1,
		Unapply = 2,
	}
	public State _state;

	// Use this for initialization
	void Start () 
	{
		_state = State.Idle;
		_alpha = 0.0f;
		GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, _alpha);
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_state == State.Apply)
		{
			if(_transitTime < _transitDuration)
			{
				_alpha = Mathf.Lerp(0.0f, 1.0f, _transitTime / _transitDuration);
				_transitTime += Time.deltaTime * StaticData.t_scale;

				GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, _alpha);
			}
			else
			{
				_state = State.Idle;
			}
		}
		else if(_state == State.Unapply)
		{
			if(_transitTime < _transitDuration)
			{
				_alpha = Mathf.Lerp(1.0f, 0.0f, _transitTime / _transitDuration);
				_transitTime += Time.deltaTime * StaticData.t_scale;
				
				GetComponent<SpriteRenderer>().color = new Color( 1.0f, 1.0f, 1.0f, _alpha);
			}
			else
			{
				_state = State.Idle;
			}
		}
	}

	public void StartApplication(float transitTime)
	{
		_transitDuration = transitTime;
		_state = State.Apply;
		_transitTime = 0.0f;
	}

	public void Unapply()
	{
		_transitDuration = 0.25f;
		_state = State.Unapply;
		_transitTime = 0.0f;
	}
}
