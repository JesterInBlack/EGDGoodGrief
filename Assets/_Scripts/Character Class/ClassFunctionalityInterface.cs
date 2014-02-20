﻿using UnityEngine;
using System.Collections;

public interface ClassFunctionalityInterface
{
	//Defines a common interface that all the character class functionality scripts must implement.

	//Button functions: on press, release, and each frame of being held
	//A and LT, RB, LB are "common denominators", 
	//  their functionality is the same across all classes, 
	//  and is defined elsewhere.

	//"Dodge" type ability
	void BPressed();
	void BReleased();
	void BHeld();

	//Tap / Hold
	void XPressed();
	void XReleased();
	void XHeld();

	//Tap / Hold
	void YPressed();
	void YReleased();
	void YHeld();

	//Tap / Hold
	void RTPressed();
	void RTReleased();
	void RTHeld();
}
