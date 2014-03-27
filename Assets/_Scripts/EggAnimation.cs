using UnityEngine;
using System.Collections;

public class EggAnimation : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		GetComponent<Animator>().speed = Random.Range( 0.25f, 1.0f );
	}
}
