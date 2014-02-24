using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public class CustomController : MonoBehaviour 
{
	//TODO: check for collision with other players
	//TODO: check for collision with boss
	//TODO: call functions in class from buttons

	#region vars
	public int facing = 3; //direction for sprites (0: Right, 1: Up, 2: Left, 3: Down)
	
	[HideInInspector]
	public bool move_enabled = true; //for disabling motion
	
	public float speed = 2.0f; //unity units per second (we're using Tile Size pixels per unit (64) )
	
	BoxCollider2D my_collider;
	ClassFunctionalityInterface actionHandler; //stores the script that handles B, X, Y, and RT functionality.
	Player playerState;

	//gamepad stuff
	bool playerIndexSet = false;
	public PlayerIndex playerIndex = PlayerIndex.One;
	GamePadState gamePadState;
	GamePadState prevGamePadState;

	private const float min_trigger_value = 0.10f; //minimum value a trigger must exceed to be considered pressed.
	#endregion

	// Use this for initialization
	void Start () 
	{
		my_collider = this.gameObject.GetComponent<BoxCollider2D>();
		playerState = this.gameObject.GetComponent<Player>();
		actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent( typeof( ClassFunctionalityInterface ) );

		#region codes
		//TODO: remove this snippet, replace with actual working code.
		GamePadState testState = GamePad.GetState ( playerIndex );
		if ( testState.IsConnected )
		{
			playerIndexSet = true;
		}
		#endregion
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region delta time
		float dt = Time.deltaTime;
		if ( ! playerState.isInBulletTime ) //check if this player is under the effects of the stopwatch.
		{
			dt = dt * StaticData.t_scale;
		}
		#endregion

		Vector2 move_vec = new Vector2( 0.0f, 0.0f );
		
		#region input parsing
		//get gamepad state
		gamePadState = GamePad.GetState ( playerIndex );

		//if controller connected, do stuff
		if ( gamePadState.IsConnected )
		{
			#region moving
			//move
			if ( move_enabled )
			{
				move_vec.x = gamePadState.ThumbSticks.Left.X;
				move_vec.y = gamePadState.ThumbSticks.Left.Y;
				move_vec.Normalize();
				playerState.carryVec = new Vector2( move_vec.x, move_vec.y ); //store normalized vec as carry vector.

				//speed stuff.
				move_vec = move_vec * playerState.speedMultiplier;

				//Downed movement logic
				if ( playerState.isDowned )
				{
					if ( playerState.isCarried )
					{
						//override move_vec
						Vector2 tempVec = playerState.carryVec + playerState.Carrier.GetComponent<Player>().carryVec;
						move_vec = 0.75f * tempVec;
					}
					else
					{
						//Crawl!
						move_vec = move_vec * 0.5f;
					}
				}

				//Carrying movement logic
				if ( playerState.isCarrier )
				{
					//override move_vec
					Vector2 tempVec = playerState.carryVec + playerState.Carrier.GetComponent<Player>().carryVec;
					move_vec = 0.75f * tempVec;
				}
			}
			#endregion

			#region aiming
			//<insert right thumbstick code here>
			#endregion

			#region buttons

			#region X
			//X was pressed
			if ( gamePadState.Buttons.X == ButtonState.Pressed && prevGamePadState.Buttons.X == ButtonState.Released )
			{
				//initialize charging
				actionHandler.XPressed();
			}
			
			//X is being held
			else if ( gamePadState.Buttons.X == ButtonState.Pressed && prevGamePadState.Buttons.X == ButtonState.Pressed )
			{
				//increment charging
				actionHandler.XHeld ( dt );
			}
			
			//X was released
			else if ( gamePadState.Buttons.X == ButtonState.Released && prevGamePadState.Buttons.X == ButtonState.Pressed )
			{
				//release charges, have effect
				actionHandler.XReleased();
			}
			#endregion
			#region Y
			//Y was pressed
			if ( gamePadState.Buttons.Y == ButtonState.Pressed && prevGamePadState.Buttons.Y == ButtonState.Released )
			{
				//initialize charging
				actionHandler.YPressed();
			}
			
			//Y is being held
			else if ( gamePadState.Buttons.Y == ButtonState.Pressed && prevGamePadState.Buttons.Y == ButtonState.Pressed )
			{
				//increment charging
				actionHandler.YHeld( dt );
			}
			
			//Y was released
			else if ( gamePadState.Buttons.Y == ButtonState.Released && prevGamePadState.Buttons.Y == ButtonState.Pressed )
			{
				//release charges, have effect
				actionHandler.YReleased();
			}
			#endregion
			#region B
			//B was pressed
			if ( gamePadState.Buttons.B == ButtonState.Pressed && prevGamePadState.Buttons.B == ButtonState.Released )
			{
				//initialize charging
				actionHandler.BPressed();
			}
			
			//B is being held
			else if ( gamePadState.Buttons.B == ButtonState.Pressed && prevGamePadState.Buttons.B == ButtonState.Pressed )
			{
				//increment charging
				actionHandler.BHeld( dt );
			}
			
			//B was released
			else if ( gamePadState.Buttons.B == ButtonState.Released && prevGamePadState.Buttons.B == ButtonState.Pressed )
			{
				//release charges, have effect
				actionHandler.BReleased();
			}
			#endregion
			#region A
			//A was pressed
			//Define behaviour here / in a common class
			if ( gamePadState.Buttons.A == ButtonState.Pressed && prevGamePadState.Buttons.A == ButtonState.Released )
			{
				//initialize charging
				//define pick up / drop
				//Pick up: contextual command?

				//DROP LOGIC
				if ( playerState.isCarried )
				{
					//force other player to drop you.
					playerState.Carrier.GetComponent<Player>().isCarrier = false;
					playerState.Carrier.GetComponent<Player>().Carried = null;
					playerState.isCarried = false;
					playerState.Carrier = null;
				}
				if ( playerState.isCarrier )
				{
					//drop player!
					playerState.Carried.GetComponent<Player>().isCarried = false;
					playerState.Carried.GetComponent<Player>().Carrier = null;
					playerState.isCarrier = false;
					playerState.Carried = null;
				}
			}

			//A is being held
			else if ( gamePadState.Buttons.A == ButtonState.Pressed && prevGamePadState.Buttons.A == ButtonState.Pressed )
			{
				//increment charging
				//define?
			}

			//A was released
			else if ( gamePadState.Buttons.A == ButtonState.Released && prevGamePadState.Buttons.A == ButtonState.Pressed )
			{
				//release charges, have effect
				//define?
			}
			#endregion

			#endregion

			#region right trigger
			//RT was pressed
			if ( gamePadState.Triggers.Right > min_trigger_value && prevGamePadState.Triggers.Right <= min_trigger_value )
			{
				//initialize charging
				actionHandler.RTPressed();
			}
			
			//RT is being held
			else if ( gamePadState.Triggers.Right > min_trigger_value && prevGamePadState.Triggers.Right > min_trigger_value )
			{
				//increment charging
				actionHandler.RTHeld ( dt );
			}
			
			//RT was released
			else if ( gamePadState.Triggers.Right <= min_trigger_value && prevGamePadState.Triggers.Right > min_trigger_value )
			{
				//release charges, have effect
				actionHandler.RTReleased();
			}
			#endregion

			#region items
			//handles LT, LB, RB
			//define behaviour here
			#region left trigger
			//LT was pressed
			if ( gamePadState.Triggers.Left > min_trigger_value && prevGamePadState.Triggers.Left <= min_trigger_value )
			{
				//initialize charging
				//use un-charge-able items.
			}
			
			//LT is being held
			else if ( gamePadState.Triggers.Left > min_trigger_value && prevGamePadState.Triggers.Left > min_trigger_value )
			{
				//increment charging
				//charge item
			}
			
			//LT was released
			else if ( gamePadState.Triggers.Left <= min_trigger_value && prevGamePadState.Triggers.Left > min_trigger_value )
			{
				//release charges, have effect
				//use chargeable items
				playerState.UseItem();
			}
			#endregion
			#region left bumper
			//LB was pressed
			if ( gamePadState.Buttons.LeftShoulder == ButtonState.Pressed && prevGamePadState.Buttons.LeftShoulder == ButtonState.Released )
			{
				//initialize charging
				playerState.ChangeItemIndex ( -1 );
			}
			
			//LB is being held
			else if ( gamePadState.Buttons.LeftShoulder == ButtonState.Pressed && prevGamePadState.Buttons.LeftShoulder == ButtonState.Pressed )
			{
				//increment charging
			}
			
			//LB was released
			else if ( gamePadState.Buttons.LeftShoulder == ButtonState.Released && prevGamePadState.Buttons.LeftShoulder == ButtonState.Pressed )
			{
				//release charges, have effect
				//do nothing.
			}
			#endregion
			#region right bumper
			//RB was pressed
			if ( gamePadState.Buttons.RightShoulder == ButtonState.Pressed && prevGamePadState.Buttons.RightShoulder == ButtonState.Released )
			{
				//initialize charging
				playerState.ChangeItemIndex ( 1 );
			}
			
			//RB is being held
			else if ( gamePadState.Buttons.RightShoulder == ButtonState.Pressed && prevGamePadState.Buttons.RightShoulder == ButtonState.Pressed )
			{
				//increment charging
			}
			
			//RB was released
			else if ( gamePadState.Buttons.RightShoulder == ButtonState.Released && prevGamePadState.Buttons.RightShoulder == ButtonState.Pressed )
			{
				//release charges, have effect
				//do nothing.
			}
			#endregion

			#endregion
		}

		//save previous gamepad state (to check for button up/down events)
		prevGamePadState = gamePadState;
		#endregion
		
		#region facing parsing
		float angle = Mathf.Rad2Deg * Mathf.Atan2 ( gamePadState.ThumbSticks.Right.Y, gamePadState.ThumbSticks.Right.X );

		//pull into range?
		if ( angle < 0.0f )
		{
			angle += 360.0f;
		}
		else if ( angle > 360.0f )
		{
			angle -= 360.0f;
		}
		
		//Based on angle, get facing.
		//TODO: only if aiming (ninja or archer while RT down)
		//TODO: if thumbstick is at (0,0), then ignore it.
		if ( (angle >= 0.0f && angle <= 45.0f) || (angle >= 315.0f && angle <= 360.0f) )
		{
			facing = 0;
		}
		else if ( angle >= 45.0f && angle <= 135.0f )
		{
			facing = 1;
		}
		else if ( angle >= 135.0f && angle <= 225.0f )
		{
			facing = 2;
		}
		else if ( angle >= 225.0f && angle <= 315.0f )
		{
			facing = 3;
		}
		
		//Debug.Log ( angle + " : " + facing ); //DEBUG LINE
		
		#endregion
		
		if ( move_vec.sqrMagnitude > 0.0f ) //sqrMagnitude is more efficient.
		{
			#region animate
			//Animate
			/*
			if ( facing == 0 ){ this.gameObject.GetComponent<Animator>().Play( "WalkRight" ); }
			else if ( facing == 1 ){ this.gameObject.GetComponent<Animator>().Play( "WalkUp" ); }
			else if ( facing == 2 ){ this.gameObject.GetComponent<Animator>().Play( "WalkLeft" ); }
			else if ( facing == 3 ){ this.gameObject.GetComponent<Animator>().Play( "WalkDown" ); }
			*/
			#endregion
			
			//Move.
			//Get direction from vec.
			
			//Scale to speed
			move_vec = move_vec * speed * dt;
			
			Move ( move_vec );
		}
		else
		{
			//Idle
			#region animate
			//this.gameObject.GetComponent<Animator>().Play("Idle");
			//Animate
			/*
			if ( facing == 0 ){ this.gameObject.GetComponent<Animator>().Play( "IdleRight" ); }
			else if ( facing == 1 ){ this.gameObject.GetComponent<Animator>().Play( "IdleUp" ); }
			else if ( facing == 2 ){ this.gameObject.GetComponent<Animator>().Play( "IdleLeft" ); }
			else if ( facing == 3 ){ this.gameObject.GetComponent<Animator>().Play( "IdleDown" ); }
			*/
			#endregion
			
		}
		
		#region animate
		//facing based-stuff
		#endregion
	}
	
	public void Move( Vector3 move_vec )
	{
		//Moves the player by move_vec (pos = pos + vec),
		//but also respects collisions.
		if ( playerState.canMove == false ) { return; }
		
		#region Collision Detection
		//Get the distance from center to edge of box collider
		float x_diff = my_collider.size.x / 2.0f + 0.01f;
		float y_diff = my_collider.size.y / 2.0f + 0.01f;
		
		//proper sign (+/-)
		if ( move_vec.x < 0 )
		{
			x_diff = x_diff * -1.0f;
		}
		if ( move_vec.y < 0 )
		{
			y_diff = y_diff * -1.0f;
		}
		
		//Collision Detection
		//TODO: isolate by axis
		bool blockedx = BlockF ( new Vector3( x_diff + move_vec.x, 0.0f, 0.0f ) ) || 
			BlockF ( new Vector3( x_diff + move_vec.x, y_diff, 0.0f ) ) ||
				BlockF ( new Vector3( x_diff + move_vec.x, y_diff * -1.0f, 0.0f) );
		
		bool blockedy = BlockF ( new Vector3( 0.0f, y_diff + move_vec.y, 0.0f ) ) ||
			BlockF ( new Vector3( x_diff, y_diff + move_vec.y, 0.0f ) ) ||
				BlockF ( new Vector3( x_diff * -1.0f, y_diff + move_vec.y, 0.0f ) );
		#endregion
		
		if ( ! blockedx )
		{
			//abstraction inefficiency
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float z = this.gameObject.transform.position.z;
			
			//Actually move
			this.gameObject.transform.position = new Vector3( x + move_vec.x, y, z );
		}
		if ( ! blockedy)
		{
			//abstraction inefficiency
			float x = this.gameObject.transform.position.x;
			float y = this.gameObject.transform.position.y;
			float z = this.gameObject.transform.position.z;
			
			//Actually move
			this.gameObject.transform.position = new Vector3( x, y + move_vec.y, z );
		}
	}
	
	bool BlockF( Vector3 vec )
	{
		//Utility Wall Block Check Function
		//For collision detection. (triple raycast)
		/*
		Debug.DrawLine( 
			this.gameObject.transform.position, 
		    this.gameObject.transform.position + vec,
		    new Color( 1.0f, 0.0f, 0.0f, 1.0f ),
		    0.033f
		);
		*/
		
		Vector3 offset = new Vector3( my_collider.center.x, my_collider.center.y, 0.0f );
		return  Physics2D.Linecast( 
		                           this.gameObject.transform.position + offset, 
		                           this.gameObject.transform.position + offset + vec, 
		                           1 << LayerMask.NameToLayer( "Wall" ) 
		                           ); 
	}
}
