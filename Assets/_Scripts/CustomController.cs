using UnityEngine;
using System.Collections;
using XInputDotNetPure; // Required in C#

public class CustomController : MonoBehaviour 
{
	//TODO: check for collision with other players
	//TODO: check for collision with boss

	#region vars
	public int facing = 3; //direction for sprites (0: Right, 1: Up, 2: Left, 3: Down)
	private int prevFacing = 3;
	
	[HideInInspector]
	public bool move_enabled = true; //for disabling motion
	[HideInInspector]
	public Vector2 move_vec = new Vector2();
	[HideInInspector]
	public float aimAngle;   //angle used for aiming a ray at an angle
	[HideInInspector]
	public Vector2 aimPoint; //point used for aiming at a position.

	[HideInInspector]
	public float speed; //unity units per second (we're using Tile Size pixels per unit (64) )
	
	BoxCollider2D my_collider;
	[HideInInspector]
	public ClassFunctionalityInterface actionHandler; //stores the script that handles B, X, Y, and RT functionality.
	private Player playerState;

	//gamepad stuff
	public PlayerIndex playerIndex = PlayerIndex.One;
	GamePadState gamePadState;
	GamePadState prevGamePadState;

	private const float min_trigger_value = 0.10f; //minimum value a trigger must exceed to be considered pressed.

	private GameObject overheadArrow;
	private GameObject reticle;
	#endregion

	// Use this for pre-initialization
	void Awake()
	{
		speed = StaticData.playerMoveSpeed;
		overheadArrow = transform.Find ( "Arrow" ).gameObject;
		reticle = transform.Find ( "Reticle" ).gameObject;
		my_collider = this.gameObject.GetComponent<BoxCollider2D>();
		playerState = this.gameObject.GetComponent<Player>();
	}

	// Use this for initialization
	void Start () 
	{
		//actionHandler = (ClassFunctionalityInterface)this.gameObject.GetComponent( typeof( ClassFunctionalityInterface ) );

		#region codes
		//TODO: remove this snippet, replace with actual working code.
		/*
		GamePadState testState = GamePad.GetState ( playerIndex );
		if ( testState.IsConnected )
		{
			playerIndexSet = true;
		}
		*/
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

		move_vec = new Vector2( 0.0f, 0.0f );
		
		#region input parsing
		//get gamepad state
		gamePadState = GamePad.GetState ( playerIndex );

		//if controller connected, do stuff
		if ( true ) //gamePadState.IsConnected )
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
				move_vec = move_vec * (playerState.speedMultiplier * playerState.speedMultiplier2);

				//Downed movement logic
				if ( playerState.isDowned )
				{
					if ( playerState.isCarried )
					{
						//override move_vec
						Vector2 tempVec = playerState.carryVec;
						if ( playerState.Carrier.GetComponent<Player>().canMove )
						{
							tempVec += playerState.Carrier.GetComponent<Player>().carryVec;
						}
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
					Vector2 tempVec = playerState.carryVec + playerState.Carried.GetComponent<Player>().carryVec;
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

			//X is resting in its natural position
			else if ( gamePadState.Buttons.X == ButtonState.Released && prevGamePadState.Buttons.X == ButtonState.Released )
			{
				//increment charging
				actionHandler.XRest ( dt );
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

			//X is resting in its natural position
			else if ( gamePadState.Buttons.Y == ButtonState.Released && prevGamePadState.Buttons.Y == ButtonState.Released )
			{
				//increment charging
				actionHandler.YRest ( dt );
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

				if ( playerState.isDowned && ! playerState.isCarried ) //if you're downed and not being carried, A revives you.
				{
					if ( playerState.HP >= StaticData.percentHPNeededToRevive * playerState.baseMaxHP )
					{
						#region ClutchScoreCheck
						//Award clutch objective points.
						int lastManStandingIndex = 0;
						int livePlayers = 0;
						for ( int i = 0; i < GameState.players.Length; i++ )
						{
							if ( GameState.players[i] != null )
							{
								if ( ! GameState.playerStates[i].isDowned )
								{
									livePlayers ++;
									lastManStandingIndex = i;
								}
							}
						}
						if ( livePlayers == 1 )
						{
							ScoreManager.Clutch ( lastManStandingIndex );
						}
						#endregion
						//revive
						playerState.isDowned = false;
						//TODO: graphic
						//TODO: animation
					}
				}
				#region drop logic
				//DROP LOGIC
				else if ( playerState.isCarried )
				{
					//force other player to drop you.
					#region animation
					playerState.Carrier.GetComponent<Animator>().Play ( "throw_" + GetComponent<Player>().GetAniSuffix() );
					playerState.Carrier.GetComponent<Player>().canMove = false;
					playerState.Carrier.GetComponent<Player>().state = "throw";
					playerState.Carrier.GetComponent<Player>().stateTimer = 0.5f;
					playerState.Carrier.GetComponent<Player>().nextState = "idle";
					#endregion
					//Debug.Log ( "Carried player made you drop them." );
					playerState.Carrier.GetComponent<Player>().isCarrier = false;
					playerState.Carrier.GetComponent<Player>().Carried = null;
					float x = transform.position.x;
					float y = transform.position.y;
					transform.position = new Vector3( x, y, 0.0f ); //reset z
					playerState.isCarried = false;
					playerState.Carrier = null;
				}
				else if ( playerState.isCarrier )
				{
					//drop player!
					#region animation
					GetComponent<Animator>().Play ( "throw_" + GetComponent<Player>().GetAniSuffix() );
					playerState.canMove = false;
					playerState.state = "throw";
					playerState.stateTimer = 0.5f;
					playerState.nextState = "idle";
					#endregion
					//Debug.Log ( "Dropped!" );
					playerState.Carried.GetComponent<Player>().isCarried = false;
					playerState.Carried.GetComponent<Player>().Carrier = null;
					float x = playerState.Carried.transform.position.x;
					float y = playerState.Carried.transform.position.y;
					playerState.Carried.transform.position = new Vector3( x, y, 0.0f ); //reset z
					playerState.isCarrier = false;
					playerState.Carried = null;
				}
				#endregion
				#region vampire logic
				//override this if vampire.
				//(set vampire state if the item connects)
				else if ( playerState.state == "vampire" )
				{
					//instead, do vampire things.
					float lifeDrain = 2.5f;
					float tempAngle = facing * Mathf.PI / 2.0f;
					float x = transform.position.x;
					float y = transform.position.y;
					float min = 0.5f; //minimum or base width & height of the hitbox
					float r = 1.0f;   //factor applied to cos / sin to extend hitbox
					//
					float xmin = Mathf.Min ( x - min, x - min + r * Mathf.Cos ( tempAngle ) );
					float ymin = Mathf.Min ( y - min, y - min + r * Mathf.Sin ( tempAngle ) );
					float xmax = Mathf.Max ( x + min, x + min + r * Mathf.Cos ( tempAngle ) );
					float ymax = Mathf.Max ( y + min, y + min + r * Mathf.Sin ( tempAngle ) );
					Collider2D[] hits = AttackSystem.getHitsInBox( new Rect( xmin, ymin, (xmax - xmin), (ymax - ymin) ), playerState.id );
					if ( hits.Length > 0 )
					{
						bool anyDrainConnected = false;
						foreach ( Collider2D hit in hits )
						{
							Player tempPlayer = hit.gameObject.GetComponent<Player>();
							LegScript tempLeg = hit.gameObject.GetComponent<LegScript>();
							BossCoreHP tempBoss = hit.gameObject.GetComponent<BossCoreHP>();
							bool drainConnected = false;
							if ( tempPlayer != null ) //is a player.
							{
								if ( tempPlayer.id != playerState.id ) //no self-hitting
								{
									tempPlayer.Hurt ( lifeDrain );
									drainConnected = true;
									anyDrainConnected = true;
									//draining from another player counts as griefing.
									GameState.cooperationAxis = Mathf.Max ( -1.0f, GameState.cooperationAxis - 0.02f );
									playerState.RemoveBuffsGivenByPlayer ( tempPlayer.id );
								}
							}
							else if ( tempLeg != null ) //is a boss leg
							{
								tempLeg.Hurt ( lifeDrain, playerState.id );
								drainConnected = true;
								anyDrainConnected = true;
							}
							else if ( tempBoss != null ) 
							{
								tempBoss.Hurt ( lifeDrain, playerState.id );
								drainConnected = true;
								anyDrainConnected = true;
							}

							if ( drainConnected )
							{
								playerState.HP = Mathf.Min ( playerState.HP + lifeDrain, playerState.maxHP );
								GetComponent<AudioSource>().PlayOneShot ( SoundStorage.ItemVampFang, 1.0f );
								GetComponent<PlayerParticleEffectManager>().EnableHealingThisFrame();
							}
						}
						if ( ! anyDrainConnected ) //no hits!
						{
							//hit failed to connect. End vampire drain mode!
							playerState.state = "idle";
							playerState.stateTimer = 0.0f;
							playerState.nextState = "idle";
						}
					}
				}
				#endregion
				#region pickup logic
				else
				{
					bool pickedup = false;
					for ( int i = 0; i < GameState.players.Length; i++ )
					{
						if ( GameState.players[i] != null )
						{
							if ( GameState.players[i].GetComponent<Player>().id != playerState.id ) //no self-carrying!
							{
								float x = GameState.players[i].gameObject.transform.position.x;
								float y = GameState.players[i].gameObject.transform.position.y;
								float myX = gameObject.transform.position.x;
								float myY = gameObject.transform.position.y;
								float dist = Mathf.Pow ( (x - myX) * (x - myX) + (y - myY) * (y - myY), 0.5f);
								if ( dist <= 1.0f ) //in range
								{
									if ( ! playerState.isCarried && ! playerState.isCarrier && ! playerState.isDowned) //you: valid state?
									{
										Player otherState = GameState.players[i].GetComponent<Player>();
										if ( ! otherState.isCarried && ! otherState.isCarrier && otherState.isDowned ) //them: valid state?
										{
											//Debug.Log ( "Carry!" );
											playerState.isCarrier = true;
											playerState.Carried = GameState.players[i];
											otherState.isCarried = true;
											otherState.Carrier = this.gameObject;
											GameState.players[i].transform.position = 
												new Vector3( 
												gameObject.transform.position.x,
											    gameObject.transform.position.y + 0.5f,
											    -1.0f
											    );
											#region animation
											playerState.canMove = false;
											playerState.state = "pickup";
											playerState.stateTimer = 0.45f;
											playerState.nextState = "idle";
											GetComponent<Animator>().Play ( "pickup_" + GetComponent<Player>().GetAniSuffix() );
											#endregion
											pickedup = true;
										}
									}
								}
							}
						}
					}
					if ( ! pickedup ) //hold A to channel healing.
					{
						if ( playerState.channellingHealingCooldown <= 0.0f ) //off cooldown
						{
							playerState.isChannellingHealing = true;
						}
					}
				}
				#endregion
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
				if ( playerState.isChannellingHealing )
				{
					playerState.isChannellingHealing = false;
				}
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
				GetComponent<ItemHandler>().ItemButtonPressed(); //use un-charge-ables, begin charging charge-ables
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
				GetComponent<ItemHandler>().ItemButtonReleased(); //use charge-ables.
			}
			#endregion
			#region left bumper
			//LB was pressed
			if ( gamePadState.Buttons.LeftShoulder == ButtonState.Pressed && prevGamePadState.Buttons.LeftShoulder == ButtonState.Released )
			{
				//initialize charging
				//make sure you're not charging an item
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
				//make sure you're not charging an item
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

			if ( gamePadState.Buttons.Back == ButtonState.Pressed && prevGamePadState.Buttons.Back == ButtonState.Released )
			{
				overheadArrow.SetActive( ! overheadArrow.activeInHierarchy );
			}
		}

		//save previous gamepad state (to check for button up/down events)
		prevGamePadState = gamePadState;
		#endregion
		
		#region facing parsing
		float angle = Mathf.Rad2Deg * Mathf.Atan2 ( gamePadState.ThumbSticks.Left.Y, gamePadState.ThumbSticks.Left.X );

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

		//if thumbstick is at (0,0), or you can't move, then ignore it.
		if ( !(  Mathf.Abs ( gamePadState.ThumbSticks.Left.Y ) < 0.01f && 
		       Mathf.Abs ( gamePadState.ThumbSticks.Left.X ) < 0.01f ) && 
		       playerState.canMove )
		{
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
		}

		//aiming override (for ninja or archer while RT down / raycasting items)
		if ( Mathf.Abs ( gamePadState.ThumbSticks.Right.Y ) > 0.0f || Mathf.Abs( gamePadState.ThumbSticks.Right.X ) > 0.0f )
		{
			aimAngle = Mathf.Rad2Deg * Mathf.Atan2 ( gamePadState.ThumbSticks.Right.Y, gamePadState.ThumbSticks.Right.X );
			aimPoint.x += gamePadState.ThumbSticks.Right.X * 10.0f * Time.deltaTime; //Mathf.Cos ( aimAngle ) * 6.0f * Time.deltaTime;
			aimPoint.y += gamePadState.ThumbSticks.Right.Y * 10.0f * Time.deltaTime; //Mathf.Sin ( aimAngle ) * 6.0f * Time.deltaTime;
			//max range check
			float maxRange = 10.0f;
			if ( Mathf.Sqrt ( (aimPoint.x - transform.position.x) * (aimPoint.x - transform.position.x) + 
			                  (aimPoint.y - transform.position.y) * (aimPoint.y - transform.position.y) ) > maxRange )
			{
				float tempAngle = Mathf.Atan2 ( aimPoint.y - transform.position.y, aimPoint.x - transform.position.x );
				aimPoint.x = transform.position.x + (maxRange - 0.01f) * Mathf.Cos ( tempAngle );
				aimPoint.y = transform.position.y + (maxRange - 0.01f) * Mathf.Sin ( tempAngle );
			}
		}
		else
		{
			aimAngle = facing * 90.0f; //default to facing
		}

		//DEBUG!
		if ( playerState.state == "item aim point" )
		{
			Debug.DrawLine( transform.position, aimPoint, new Color( 0.0f, 0.0f, 1.0f, 1.0f ) );
			reticle.GetComponent<SpriteRenderer>().enabled = true;
			reticle.transform.position = new Vector3( aimPoint.x, aimPoint.y, 2.0f );
		}
		else
		{
			reticle.GetComponent<SpriteRenderer>().enabled = false;
		}
		//Debug.Log ( angle + " : " + facing ); //DEBUG LINE
		
		#endregion

		if ( move_vec.sqrMagnitude > 0.0f ) //sqrMagnitude is more efficient.
		{
			if ( playerState.state == "idle" )
			{
				playerState.state = "walk";
				playerState.stateTimer = 0.0f;
				playerState.nextState = "walk";
			}

			//animate
			if ( playerState.state == "walk" )
			{
				#region animation
				if ( ! playerState.isDowned )
				{
					if ( playerState.isCarried == false && playerState.isCarrier == false ) //default
					{
						gameObject.GetComponent<Animator>().Play( "walk_" + playerState.GetAniSuffix() );
					}
					else if ( playerState.isCarrier ) //carrying another player
					{
						gameObject.GetComponent<Animator>().Play( "carry_" + playerState.GetAniSuffix() );
					}
					else if ( playerState.isCarried ) //being carried by another player
					{
						gameObject.GetComponent<Animator>().Play( "idle_" + playerState.GetAniSuffix() );
					}
				}
				else
				{
					gameObject.GetComponent<Animator>().Play( "crawl_" + playerState.GetAniSuffix() );
				}
				#endregion
			}
			else if ( playerState.state == "item charge" || 
			          playerState.state == "item aim ray" || 
			          playerState.state == "item aim point" )
			{
				gameObject.GetComponent<Animator>().Play( "carry_" + playerState.GetAniSuffix() );
			}
			
			//Move.
			//Get direction from vec.
			
			//Scale to speed
			move_vec = move_vec * speed * dt;
			
			Move ( move_vec );
		}
		else
		{
			if ( playerState.state == "walk" )
			{
				playerState.state = "idle";
				playerState.stateTimer = 0.0f;
				playerState.nextState = "idle";
			}

			if ( playerState.state == "idle" )
			{
				#region animate
				if ( facing == 0 && prevFacing != 0 )
				{
					gameObject.GetComponent<Animator>().Play( "idle_right" );
				}
				else if ( facing == 1 && prevFacing != 1 )
				{
					gameObject.GetComponent<Animator>().Play( "idle_up" );
				}
				else if ( facing == 2 && prevFacing != 2 )
				{
					gameObject.GetComponent<Animator>().Play( "idle_left" );
				}
				else if ( facing == 3 && prevFacing != 3 )
				{
					gameObject.GetComponent<Animator>().Play( "idle_down" );
				}
				#endregion
			}
			else if ( playerState.state == "item charge" || 
			         playerState.state == "item aim ray" || 
			         playerState.state == "item aim point" )
			{
				gameObject.GetComponent<Animator>().Play( "carry_idle_" + playerState.GetAniSuffix() );
			}
			
		}
		
		#region animate
		//facing based-stuff
		#endregion

		prevFacing = facing;
	}
	
	public void Move( Vector3 move_vec )
	{
		//Moves the player by move_vec (pos = pos + vec),
		//but also respects collisions.
		if ( playerState.canMove == false ) { return; }
		MoveNaoPlz( move_vec );
	}

	public void MoveNaoPlz( Vector3 move_vec )
	{
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
		                           //( (1 << LayerMask.NameToLayer( "Wall" ) ) | (1 << LayerMask.NameToLayer ( "Player" ) ) )
		                           ); 
	}
}
