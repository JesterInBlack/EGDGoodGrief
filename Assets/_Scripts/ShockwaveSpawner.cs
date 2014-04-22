using UnityEngine;
using System.Collections;

public class ShockwaveSpawner : MonoBehaviour 
{
	//the object with the particle components
	public Transform _ringEffect;

	//expansion speed of the ring
	public float _speed = 6.5f;

	//the inner and outer radii determine the width of the ring
	public float _innerRadius = 0.5f;
	public float _outterRadius = 1.5f;

	private float _lifeTime;
	private float _timer;

	// Use this for initialization
	void Start ()
	{
		_timer = 0.0f;
		_lifeTime = _ringEffect.GetComponent<ParticleEmitter>().maxEnergy;
	}
	
	// Update is called once per frame
	void Update () 
	{
		/*
		if(Input.GetKeyDown(KeyCode.A))
		{
			SpawnShockwave(transform.position);
		}
		*/
		if(_timer == 0.0f)
		{
			Vector3 targetPos = new Vector3(transform.position.x, transform.position.y, 0.0f);
			SpawnShockwave(targetPos);
		}
		if(_timer < _lifeTime)
		{
			_timer += Time.deltaTime * StaticData.t_scale;
		}
		else
		{
			Destroy(this.gameObject);
		}

	}

	//this function creates a shockwave at the target vector each time it's called
	public void SpawnShockwave(Vector3 spawnPoint)
	{
		Transform effectObject = Instantiate(_ringEffect, spawnPoint, this.transform.rotation) as Transform;

		effectObject.transform.parent = this.gameObject.transform;

		//get the particle emitter from the new effect object
		ParticleEmitter emitter = effectObject.GetComponent<ParticleEmitter>();

		//generate the particles
		emitter.Emit();

		Particle[] p =  emitter.particles;
		//loop through particles giving intial vel and pos
		for(int i = 0; i < p.Length; i++)
		{
			Vector3 ruv = RandomUnitInVectorPlane(effectObject.transform, effectObject.transform.forward);

			// Calc the initial position of the particle accounting for the specified ring radii.  Note the use of Range
			// to get a random distance distribution within the ring.
			Vector3 newPos = effectObject.transform.position + ((ruv * _innerRadius) + (ruv * Random.Range(_innerRadius, _outterRadius)));
			p[i].position = newPos;

			//the velocity vector is simply the unit vector modified by the speed.
			//the velocity vector is used by the Particle Animator component to move the particles
			p[i].velocity = ruv * _speed;
		}

		//update the particles
		emitter.particles = p;
	}

	Vector3 RandomUnitInVectorPlane(Transform xform, Vector3 axis)
	{
		//rotate the specified tranform's axis though a random angle
		xform.Rotate(axis, Random.Range(0.0f, 360.0f), Space.World);

		//get a copy of the rotated axis and normalize it
		Vector3 ruv = new Vector3(xform.right.x, xform.right.y, xform.right.z);
		ruv.Normalize();
		return ruv;
	}
}
