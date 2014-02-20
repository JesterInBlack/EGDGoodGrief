using UnityEngine;
using System.Collections;

public class RocketSwordFunctions : MonoBehaviour, ClassFunctionalityInterface 
{
	//This class defines the moves for the rocket sword class.
	//It reaches into the base player class to modify certain attributes.
	//It reaches into the custom controller class to modify movement 
	//(TODO: make custom controller reach into base player)

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
		//Initialize parry.
	}
	public void BReleased()
	{
		//Called when B is released.
		//DO NOTHING.
	}
	public void BHeld()
	{
		//Called every frame B is held down.
		//DO NOTHING.
	}
	
	//Tap / Hold
	public void XPressed()
	{
		//Called when X is pressed.
		//Initialize horizontal slash.
		//OR: prolong hurricane spin.
	}
	public void XReleased()
	{
		//Called when X is released.
		//Do horizontal slash / hurricane spin.
	}
	public void XHeld()
	{
		//Called every frame X is held down.
		//Charge horizontal slash -> hurricane spin.
	}
	
	//Tap / Hold
	public void YPressed()
	{
		//Called when Y is pressed.
		//Initialize overhead strike.
	}
	public void YReleased()
	{
		//Called when Y is released.
		//Do overhead strike / blastoff
	}
	public void YHeld()
	{
		//Called every frame Y is held down.
		//Charge overhead strike -> blastoff
	}
	
	//Tap / Hold
	public void RTPressed()
	{
		//Called when RT is pressed.
		//Initialize rev charge.
		//Slow speed.
	}
	public void RTReleased()
	{
		//Called when RT is released.
		//Set up no degen window.
		//Reset speed.
	}
	public void RTHeld()
	{
		//Called every frame RT is held down.
		//Increase rev charge slightly?
	}
}
