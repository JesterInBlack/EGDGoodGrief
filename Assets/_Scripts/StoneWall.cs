using UnityEngine;
using System.Collections;

public class StoneWall : MonoBehaviour 
{
	//A class used for the monk's stone wall effect.
	#region vars
	public int facing;

	private bool deleteMe = false;
	private float timer = 0.0f;
	#endregion

	// Use this for initialization
	void Start () 
	{
		if ( facing == 0 || facing == 1 || facing == 2 || facing == 3 )
		{
			GetComponent<Animator>().Play ( "rise_" + GetAniSuffix () );
		}
		else
		{
			Debug.LogError( "Invalid stone wall facing" );
		}

		if ( facing == 3 )
		{
			GetComponent<SpriteRenderer>().sortingLayerName = "Above";
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		float dt = Time.deltaTime * StaticData.t_scale;
		timer += dt;

		if ( timer >= 0.6f && deleteMe )
		{
			Destroy ( this.gameObject );
		}
	}

	public void Shatter()
	{
		GetComponent<Animator>().Play ( "break_" + GetAniSuffix () );
		deleteMe = true;
		timer = 0.0f;
	}

	private string GetAniSuffix()
	{
		//returns a string to append to a base animation name to get the correct direction.
		if ( facing == 0 )
		{
			return "right";
		}
		else if ( facing == 1 )
		{
			return "up";
		}
		else if ( facing == 2 )
		{
			return "left";
		}
		else if ( facing == 3 )
		{
			return "down";
		}
		//default
		return "error";
	}
}
