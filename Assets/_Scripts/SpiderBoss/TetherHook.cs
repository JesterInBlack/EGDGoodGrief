using UnityEngine;
using System.Collections;

public class TetherHook : MonoBehaviour 
{
	public GameObject _parent;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position = _parent.transform.position;
	}
}
