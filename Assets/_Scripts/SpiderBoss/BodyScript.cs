using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BodyScript : MonoBehaviour {

	public Vector2 move_vec = new Vector2(0, 0);

	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 3f;				// The fastest the player can travel in the x axis.

	public GameObject RightLeg1;

	private List<GameObject> legs = new List<GameObject>();

	// Use this for initialization
	void Start () 
	{
		legs.Add(RightLeg1);
	}
	
	// Update is called once per frame
	void Update () 
	{

		move_vec.x = Input.GetAxis("Horizontal");
		move_vec.y = Input.GetAxis("Vertical");
		move_vec.Normalize();
		{
			//abstraction inefficiency
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float z = this.gameObject.transform.position.z;
			
			//Actually move
			this.gameObject.transform.position = new Vector3( x + move_vec.x * 0.05f, y, z );
		}

		{
			//abstraction inefficiency
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float z = this.gameObject.transform.position.z;
			
			//Actually move
			this.gameObject.transform.position = new Vector3( x, y + move_vec.y * 0.05f, z );
		}

		/*
		// Cache the horizontal input.
		float h = Input.GetAxis("Horizontal");
		
		// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		if(h * rigidbody2D.velocity.x < maxSpeed)
		{
			// ... add a force to the player.
			rigidbody2D.AddForce(Vector2.right * h * moveForce);
		}
		
		// If the player's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(rigidbody2D.velocity.x) > maxSpeed)
		{
			// ... set the player's velocity to the maxSpeed in the x axis.
			rigidbody2D.velocity = new Vector2(Mathf.Sign(rigidbody2D.velocity.x) * maxSpeed, rigidbody2D.velocity.y);
		}
		if(h == 0)
		{
			Vector2 bluh = new Vector2(0.0f, rigidbody2D.velocity.y);
			rigidbody2D.velocity  = bluh;
		}

		
		float v = Input.GetAxis("Vertical");
		// If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
		if(v * rigidbody2D.velocity.y < maxSpeed)
		{
			// ... add a force to the player.
			rigidbody2D.AddForce(Vector2.up * v * moveForce);
		}
		
		// If the player's horizontal velocity is greater than the maxSpeed...
		if(Mathf.Abs(rigidbody2D.velocity.y) > maxSpeed)
		{
			// ... set the player's velocity to the maxSpeed in the x axis.
			rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, Mathf.Sign(rigidbody2D.velocity.y) * maxSpeed);
		}
		if(v == 0)
		{
			Vector2 bluh = new Vector2(rigidbody2D.velocity.x, 0.0f);
			rigidbody2D.velocity  = bluh;
		}
		*/

	}
}
