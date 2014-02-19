using UnityEngine;
using System.Collections;

public class Boss : MonoBehaviour 
{
	//Class to handle boss AI, etc.

	#region vars
	public float hp;
	public float maxhp;

	//complexity: AI, aggro, attack logic, stats by part?
	#endregion

	// Use this for initialization
	void Start () 
	{
		hp = maxhp;
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
}
