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
		//Probably just animation + state vars
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

		//Animation + state vars
	}
	public void XReleased()
	{
		//Called when X is released.
		//Do horizontal slash / hurricane spin.

		//Animation + state vars
		//poll gamestate, check for sector hit detection
		//hurricane spin makes the sector thing a bit more complex.
		//would it be better to poll gamestate, or try physics raycast type deal?
	}
	public void XHeld()
	{
		//Called every frame X is held down.
		//Charge horizontal slash -> hurricane spin.

		//Animation + state vars
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
		//Animation + state vars
		//poll gamestate, do hitbox hit detection.
		//with the charge, hitbox detection over time. A bit more complex?
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
