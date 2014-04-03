using UnityEngine;
using System.Collections;

public class RadiusScript : MonoBehaviour 
{
	public GameObject _parent;
	private Vector3 _pos = new Vector3(0, 0, 0);
	private Vector3 _offset;
	
	// Use this for initialization
	void Start () 
	{
		_offset = transform.position;
	}
	
	// Update is called once per frame
	void Update () 
	{
		_pos = _parent.transform.position;
		_pos += _offset;
		transform.position = _pos;
	}
}
