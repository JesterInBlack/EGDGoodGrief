using UnityEngine;
using System.Collections;

public class EyeLaserScript : MonoBehaviour 
{

	private Vector3 _startPos;
	//private float _rotationAngle;
	public float _travelDistance = 20.0f;
	public float _speed = 3.5f;

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
		AttackSystem.hitCircle( transform.position, 1.0f, 1.0f * Time.deltaTime * StaticData.t_scale, -1 );
		if(Vector3.Distance(_startPos, transform.position) > _travelDistance)
		{
			Destroy(this.gameObject);
		}
		else
		{
			transform.position += transform.right * _speed * Time.deltaTime * StaticData.t_scale;
		}
	}

	public void Initializer(Vector3 startPos, float rotationAngle)
	{
		transform.position = startPos;
		_startPos = transform.position;
		transform.eulerAngles = new Vector3(0, 0, rotationAngle);
	}
}
