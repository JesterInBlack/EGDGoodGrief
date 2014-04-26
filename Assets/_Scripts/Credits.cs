using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour 
{
	public TextMesh names;
	public TextMesh headings;

	// Use this for initialization
	void Start () 
	{
		names.text = 
			"CREDITS\n" +
			"\n" +
			"\n" +
			"\n" +
			"PRODUCTION\n" +
			"Executive Producer:\n" +
			"Marcus Baker\n" +
			"\n" +
			"\n" +
			"DESIGN\n" +
			"Lead Designers:\n" +
			"Jay Lee\n" +
			"Gabriel Violette\n" +
			"\n" +
			"HUD Designer:\n" +
			"Pj Castracucco\n" +
			"\n" +
			"Designers:\n" +
			"Marcus Baker\n" +
			"Victor Ariel Cortes\n" +
			"\n" +
			"\n" +
			"PROGRAMMING\n" +
			"Character Programming:\n" +
			"Gabriel Violette\n" +
			"\n" +
			"Boss Programming:\n" +
			"Jay Lee\n" +
			"\n" +
			"Menu Programming:\n" +
			"Andrew Ryther\n" +
			"\n" +
			"\n" +
			"ART\n" +
			"Super Artist:\n" +
			"Victor Cortes\n" +
			"\n" +
			"Artists:\n" +
			"Pj Castracucco\n" +
			"Jay Lee\n" +
			"Gabriel Violette\n" +
			"\n" +
			"\n" +
			"SOUND EFFECTS\n" +
			"Andrew Ryther\n" +
			"Mark DiAngelo\n" +
			"Mike Koenig\n" +
			"\n" +
			"\n" +
			"MUSIC\n" +
			"Composers:\n" +
			"Johnathan Reed\n" +
			"James Ross\n" +
			"\n" +
			"\n" +
			"QUALITY ASSURANCE\n" +
			"Quality Assurance Leads:\n" +
			"Marcus Baker\n"+
			"Pj Castracucco \n" +
			"\n" +
			"Quality Assurance:\n" +
			"Kori Lutz\n" +
			"Will Nemiroff\n" +
			"Jack McNally\n" +
			"Jeff Patrick\n" +
			"Phil Zeng\n" +
			"Chris Georgi\n" +
			"Aesa Kamar\n" +
			"Jacob Martin\n" +
			"Lucas Zeratl\n" +
			"\n" +
			"\n" +
			"SPECIAL THANKS\n" +
			"All 360 No Scope Fans\n" +
			"\n" +
			"\n" +
			"Grief for Glory copyright 2014 Team 360 No Scope";

		headings.text = 
			"CREDITS\n" +
			"\n" +
			"\n" +
			"\n" +
			"PRODUCTION\n" +
			"Executive Producer:\n" +
			"\n" +
			"\n" +
			"\n" +
			"DESIGN\n" +
			"Lead Designers:\n" +
			"\n" +
			"\n" +
			"\n" +
			"HUD Designer:\n" +
			"\n" +
			"\n" +
			"Designers:\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"PROGRAMMING\n" +
			"Character Programming:\n" +
			"\n" +
			"\n" +
			"Boss Programming:\n" +
			"\n" +
			"\n" +
			"Menu Programming:\n" +
			"\n" +
			"\n" +
			"\n" +
			"ART\n" +
			"Super Artist:\n" +
			"\n" +
			"\n" +
			"Artists:\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"SOUND EFFECTS\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"MUSIC\n" +
			"Composers:\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"QUALITY ASSURANCE\n" +
			"Quality Assurance Leads:\n" +
			"\n"+
			"\n" +
			"\n" +
			"Quality Assurance:\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"\n" +
			"SPECIAL THANKS";
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position += new Vector3( 0.0f, Time.deltaTime, 0.0f );

		if ( transform.position.y > 47.0f ) //credits are done.
		{
			Application.LoadLevel ( "MainMenu" );
		}
	}
}
