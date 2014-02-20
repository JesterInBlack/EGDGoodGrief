using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour 
{
	#region vars
	public string name;
	public float CoolDownDelay = 0.0f;  //How long the item takes to cool down
	public float CoolDownTimer = 0.0f;  //Current status ( 0.0f = ready )
	#endregion

	//TODO: add an interface for calling functions?

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		float t = Time.deltaTime * StaticData.t_scale;
		CoolDownTimer = Mathf.Max ( CoolDownTimer - t, 0.0f );
	}
}
