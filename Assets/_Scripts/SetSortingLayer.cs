using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour 
{
	public string sortingLayerName;

	// Use this for initialization
	void Start () 
	{
		renderer.sortingLayerName = sortingLayerName;
	}
}
