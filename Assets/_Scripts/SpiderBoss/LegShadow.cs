using UnityEngine;
using System.Collections;

public class LegShadow : MonoBehaviour 
{
	public GameObject _shadow;
	public GameObject _parentLeg;
	private LegScript _legScript;
	private Vector3 _pos = new Vector3(0, 0, 0);
	private bool _enabled;

	// Use this for initialization
	void Start () 
	{
		_enabled = true;
		_legScript = _parentLeg.GetComponent<LegScript>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		_pos = _legScript._shadowPos;
		_pos.z = 10.0f;
		transform.position = _pos;

		if(_enabled == true && _legScript._behaviorState == LegScript.BehaviorState.Disabled)
		{
			_enabled = false;
			_shadow.SetActive(false);
		}
		if(_enabled == false && _legScript._behaviorState != LegScript.BehaviorState.Disabled)
		{
			_enabled = true;
			_shadow.SetActive(true);
		}
	}
}
