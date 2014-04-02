using UnityEngine;
using System.Collections;

public class BodyShadow : MonoBehaviour
{
	public GameObject _parent;
	private BodyScript _bodyScript;
	private Vector3 _pos = new Vector3(0, 0, 0);
	
	// Use this for initialization
	void Start () 
	{
		_bodyScript = _parent.GetComponent<BodyScript>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		_pos = _bodyScript._shadowPos;
		_pos.z = 10.0f;
		transform.position = _pos;
	}
}
