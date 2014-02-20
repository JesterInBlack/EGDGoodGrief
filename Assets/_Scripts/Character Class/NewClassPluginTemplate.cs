using UnityEngine;
using System.Collections;

public class NewClassPluginTemplate : MonoBehaviour, ClassFunctionalityInterface 
{
	//Template for adding new classes to the game:
	//Blank template to fill out the ClassFunctionalityInterface

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{

	}
	
	//"Dodge" type ability
	public void BPressed()
	{
		//Called when B is pressed.
	}
	public void BReleased()
	{
		//Called when B is released.
	}
	public void BHeld()
	{
		//Called every frame B is held down.
	}
	
	//Tap / Hold
	public void XPressed()
	{
		//Called when X is pressed.
	}
	public void XReleased()
	{
		//Called when X is released.
	}
	public void XHeld()
	{
		//Called every frame X is held down.
	}
	
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
	}
	public void YReleased()
	{
		//Called when Y is released.
	}
	public void YHeld()
	{
		//Called every frame Y is held down.
	}
	
	//Tap / Hold
	public void RTPressed()
	{
		//Called when RT is pressed.
	}
	public void RTReleased()
	{
		//Called when RT is released.
	}
	public void RTHeld()
	{
		//Called every frame RT is held down.
	}
}
