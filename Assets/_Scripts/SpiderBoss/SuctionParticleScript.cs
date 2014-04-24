using UnityEngine;
using System.Collections;

public class SuctionParticleScript : MonoBehaviour 
{
	private bool _startDying;
	private float _deathTimer;
	private float _deathDuration;

	// Use this for initialization
	void Start () 
	{
		_deathDuration = 1.5f; 
		_deathTimer = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(_startDying == true)
		{
			if(_deathTimer < _deathDuration)
			{
				_deathTimer += Time.deltaTime * StaticData.t_scale;
			}
			else
			{
				Destroy (this.gameObject);
			}
		}
	}
	public void Kill()
	{
		ParticleSystem[] temp = GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem s in temp)
		{
			s.enableEmission = false;
		}

		_startDying = true;
	}
}
