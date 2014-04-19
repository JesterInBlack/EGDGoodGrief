using UnityEngine;
using System.Collections;

public class Credits : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{
		GetComponent<TextMesh>().text = 
			"CREDITS\n" +
			"\tProduction:\n" +
			"\t\tExecutive Producer:\n" +
			"\t\t\tMarcus 'Dough Puncher' Baker\n" +
			"\n" +
			"\n" +
			"\tDesign:\n" +
			"\t\tLead Designers:\n" +
			"\t\t\tJay Ree\n" +
			"\t\t\tGabriel Violette\n" +
			"\n" +
			"\t\tHUD Designer:\n" +
			"\t\t\tPj Castracucco\n" +
			"\n" +
			"\t\tDesigners:\n" +
			"\t\t\tMarcus Baker\n" +
			"\t\t\tVictor Ariel Cortes\n" +
			"\n" +
			"\n" +
			"\tProgramming:\n" +
			"\t\tPlayer Programming:\n" +
			"\t\t\tGabriel Violette\n" +
			"\n" +
			"\t\tBoss Programming:\n" +
			"\t\t\tJay Lee\n" +
			"\n" +
			"\t\tMenu Programming:\n" +
			"\t\t\tAndrew Ryther\n" +
			"\n" +
			"\n" +
			"\tArt:\n" +
			"\t\tSuper Artist:\n" +
			"\t\t\tVictor Cortes\n" +
			"\n" +
			"\t\tArtists:\n" +
			"\t\t\tPj Castracucco\n" +
			"\t\t\tJay Lee\n" +
			"\t\t\tGabriel Violette\n" +
			"\n" +
			"\n" +
			"\tFoley:\n" +
			"\t\tAndrew Ryther\n" +
			"\n" +
			"\n" +
			"\tMusic:\n" +
			"\t\tComposers:\n" +
			"\t\t\tJames Ross\n" +
			"\t\t\tThat Other Guy\n" +
			"\n" +
			"\n" +
			"\tQuality Assurance:\n" +
			"\t\tQuality Assurance Leads:\n" +
			"\t\t\tMarcus Baker\n"+
			"\t\t\tPj Castracucco \n" +
			"\n" +
			"\t\tQuality Assurance:\n" +
			"\t\t\t<insert playtester names>\n" +
			"\n" +
			"\n" +
			"\tSPECIAL THANKS\n" +
			"\t\tAll 360 No Scope Fans\n" +
			"\n" +
			"\n" +
			"Grief for Glory copyright 2014 Team 360 No Scope";
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.position += new Vector3( 0.0f, Time.deltaTime, 0.0f );
	}
}
