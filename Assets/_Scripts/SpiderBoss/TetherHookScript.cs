using UnityEngine;
using System.Collections;

public class TetherHookScript : MonoBehaviour 
{
	public GameObject _hook;
	public float _rotationSpeed = 175.0f;
	public float _targetAngle;	
	public float _stretchScale;

	public Vector3 _rotationVec = new Vector3(0, 0, 0);
	public Vector3 _stretchVec = new Vector3(1, 1, 1);
	// Use this for initialization
	void Start () 
	{
		_stretchScale = 0.0f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		GetTargetAngle(_hook.transform.position);
		RotateToTarget(_rotationSpeed);
		Stretch();
	}

	public void GetTargetAngle(Vector3 targetPos)
	{
		float deltaX = targetPos.x - transform.position.x;
		float deltaY = targetPos.y - transform.position.y;
		
		float angleInDeg = Mathf.Atan2(deltaY, deltaX) * 180 / Mathf.PI;
		
		_targetAngle = angleInDeg;
	}

	public void RotateToTarget(float rotSpeed)
	{
		_rotationVec.z = Mathf.MoveTowardsAngle(transform.eulerAngles.z, _targetAngle, rotSpeed);
		transform.eulerAngles = _rotationVec;
	}
	public void Stretch()
	{
		_stretchScale = Vector2.Distance((Vector2)transform.position, (Vector2)_hook.transform.position) / GetComponent<SpriteRenderer>().sprite.bounds.size.x;
		_stretchVec.x = _stretchScale;
		transform.localScale = _stretchVec;
	}
}
