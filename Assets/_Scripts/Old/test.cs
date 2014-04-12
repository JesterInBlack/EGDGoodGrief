using UnityEngine;
using System.Collections;

public class test : MonoBehaviour {
	
	public Info_Script holder;

	public GUIStyle myStyle;
	float time = 0;
	bool main = true;
	bool start = false;
	bool item = false;
	string[] menuOptions = new string[] {"Start Game", "Options", "Exit"};
	string[] char_des = new string[] {"Knight", "Monk", "Archer", "???"};
	int selectedIndex = 0;
	int player_count = 1;

	int charIndex = 0;
	int charIndex2 = -1;
	int charIndex3 = -1;
	int charIndex4 = -1;

	int charIindex  = 0;
	int charIindex2 = 0;
	int charIindex3 = 0;
	int charIindex4 = 0;
	
	public Vector2 scrollPosition1 = Vector2.zero;
	public Vector2 scrollPosition2 = Vector2.zero;
	public Vector2 scrollPosition3 = Vector2.zero;
	public Vector2 scrollPosition4 = Vector2.zero;

	public Texture2D char_back;
	public Texture2D item_back;

	public Texture2D player1symbol;
	public Texture2D player2symbol;
	public Texture2D player3symbol;
	public Texture2D player4symbol;
	

	Texture2D[] charnums;
	bool player2 = false;
	bool player3 = false;
	bool player4 = false;

	bool[][] supply_matrix;
	bool[] item1_supply = new bool[] { false, false, false, false};
	bool[] item2_supply = new bool[] { false, false, false, false};
	bool[] item3_supply = new bool[] { false, false, false, false};
	bool[] item4_supply = new bool[] { false, false, false, false};
	bool[] item5_supply = new bool[] { false, false, false, false};
	bool[] item6_supply = new bool[] { false, false, false, false};
	bool[] item7_supply = new bool[] { false, false, false, false};
	bool[] item8_supply = new bool[] { false, false, false, false};
	bool[] item9_supply = new bool[] { false, false, false, false};
	bool[] item10_supply = new bool[] { false, false, false, false};
	bool[] item11_supply = new bool[] { false, false, false, false};
	bool[] item12_supply = new bool[] { false, false, false, false};

	bool[] char_supply = new bool[] { false, false, false, false,false, false,false, false, false, false,false, false};
	bool[] char_supply2 = new bool[] { false, false, false, false,false, false,false, false, false, false,false, false};
	bool[] char_supply3 = new bool[] { false, false, false, false,false, false,false, false, false, false,false, false};
	bool[] char_supply4 = new bool[] { false, false, false, false, false, false,false, false, false, false,false, false};

	int player1_items = 0;
	int player2_items = 0;
	int player3_items = 0;
	int player4_items = 0;

	void Start()
	{
		charnums = new Texture2D[] {player1symbol, player2symbol, player3symbol, player4symbol};
		supply_matrix = new bool[][]{item1_supply, item2_supply, item3_supply, item4_supply, item5_supply, item6_supply, item7_supply, item8_supply, item9_supply, item10_supply, item11_supply, item12_supply};
	}


	int menuSelection (int selectedItem, string direction) 
	{
		if (Time.time - time > 0.25)
		{
			if (direction == "up") 
			{
				if (selectedItem == 0) 
				{
					selectedItem = 2;
				}	 
				else 
				{
					selectedItem -= 1;	
				}
				time = Time.time;
			}
		
			if (direction == "down")
			{
				if (selectedItem == 2) 
				{
					selectedItem = 0;
				} 
				else 
				{
					selectedItem += 1;
				}
				time = Time.time;
			}
		}
		return selectedItem;
	}

	int menuSelection2 (int selectedItem, string direction) 
	{
		if (Time.time - time > 0.25)
		{
			if (direction == "left") 
			{
				if (selectedItem == 0) 
				{
					selectedItem = 11;
				}	 
				else 
				{
					selectedItem -= 1;	
				}
				time = Time.time;
			}
			
			if (direction == "right")
			{
				if (selectedItem == 11) 
				{
					selectedItem = 0;
				} 
				else 
				{
					selectedItem += 1;
				}
				time = Time.time;
			}
		}
		return selectedItem;
	}

	void Update ()
	{
		if (Input.GetButtonDown("360_AButton2"))
		    print ("fuuuuuuuuuuu");

		if (main == true)
		{
			if (Input.GetAxis("360_LAY") == -1) 
			{
				selectedIndex = menuSelection(selectedIndex, "up");
			}
	
			if (Input.GetAxis("360_LAY") == 1) 
			{
				selectedIndex = menuSelection( selectedIndex, "down");
			}
	
			if (Input.GetButtonDown("360_AButton")) 
			{
				handleselection();
			}
		}

		else if (start == true)
		{
			//player 1 character select;
			if (Input.GetButtonDown("360_BButton"))
			{
				start = false;
				main = true;
			}
			if (Input.GetAxisRaw("360_LAX") == -1) 
			{
				charIndex = menuSelection(charIndex, "up");
			}

			if (Input.GetAxisRaw("360_LAX") == 1) 
			{
				charIndex = menuSelection(charIndex, "down");
			}
			if (Input.GetButtonDown("360_StartButton"))
			{
				start = false;
				item = true;
			}
			//player 2 character select:
			if (player2 == false)
			{
				if(Input.GetButtonDown("360_AButton2"))
				{
					charIndex2 = 0;
					player2 = true;
					player_count++;
				}
			}
			else if(player2 == true)
			{
				if(Input.GetButtonDown("360_BButton2"))
				{
					player2 = false;
					charIndex2 = -1;
					player_count--;
				}

				if (Input.GetAxisRaw("360_LAX2") == -1) 
				{
					charIndex2 = menuSelection(charIndex2, "up");
				}
				
				if (Input.GetAxisRaw("360_LAX2") == 1) 
				{
					charIndex2 = menuSelection(charIndex2, "down");
				}
			}
			//player 3 character select:
			if(Input.GetButtonDown("360_AButton3"))
			{
				charIndex3 = 0;
				player3 = true;
				player_count++;
			}
			if(player3 == true)
			{
				if(Input.GetButtonDown("360_BButton3"))
				{
					charIndex3 = -1;
					player3 = false;
					player_count--;
				}
				
				if (Input.GetAxisRaw("360_LAX3") == -1) 
				{
					charIndex3 = menuSelection(charIndex3, "up");
				}
				
				if (Input.GetAxisRaw("360_LAX3") == 1) 
				{
					charIndex3 = menuSelection(charIndex3, "down");
				}
			}
			//player 4 character select:
			if(Input.GetButtonDown("360_AButton4"))
			{
				charIndex4 = 0;
				player4 = true;
				player_count++;
			}
			if(player4 == true)
			{
				if(Input.GetButtonDown("360_BButton4"))
				{
					charIndex4 = -1;
					player4 = false;
					player_count--;
				}
				
				if (Input.GetAxisRaw("360_LAX4") == -1) 
				{
					charIndex4 = menuSelection(charIndex4, "up");
				}
				
				if (Input.GetAxisRaw("360_LAX4") == 1) 
				{
					charIndex4 = menuSelection(charIndex4, "down");
				}
			}

		}

		else if (item == true)

		{
			//player 1 item select;
			if (Input.GetButtonDown("360_BButton"))
			{
				item = false;
				start = true;
			}
			if (Input.GetAxisRaw("360_LAX") == -1) 
			{
				charIindex = menuSelection2(charIindex, "left");
			}
			
			if (Input.GetAxisRaw("360_LAX") == 1) 
			{
				charIindex = menuSelection2(charIindex, "right");
			}

			if (Input.GetButtonDown("360_AButton"))
			{
				if (char_supply[charIindex] == true)
				{
					player1_items--;
					char_supply[charIindex] = false;
					supply_matrix[charIindex][0] = false;
				}

				else if (char_supply[charIindex] == false && player1_items < 3)
				{
					player1_items++;
					char_supply[charIindex] = true;
					supply_matrix[charIindex][0] = true;
				}

			}

			if (Input.GetButtonDown("360_StartButton"))
			{
				item = false;
				begin();
			}
			//player 2 item select:

			if (Input.GetAxisRaw("360_LAX2") == -1) 
			{
				charIindex2 = menuSelection2(charIindex2, "left");
			}
			
			if (Input.GetAxisRaw("360_LAX2") == 1) 
			{
				charIindex2 = menuSelection2(charIindex2, "right");
			}
			
			if (Input.GetButtonDown("360_AButton2"))
			{
				if (char_supply2[charIindex2] == true)
				{
					player2_items--;
					char_supply2[charIindex2] = false;
					supply_matrix[charIndex2][1] = false;
				}
				
				else if (char_supply2[charIindex2] == false && player2_items < 3)
				{
					player2_items++;
					char_supply2[charIindex2] = true;
					supply_matrix[charIndex2][1] = true;
				}
				
			}

			//player 3 item select:
			if (Input.GetAxisRaw("360_LAX3") == -1) 
			{
				charIindex3 = menuSelection2(charIindex3, "left");
			}
			
			if (Input.GetAxisRaw("360_LAX3") == 1) 
			{
				charIindex3 = menuSelection2(charIindex3, "right");
			}
			
			if (Input.GetButtonDown("360_AButton3"))
			{
				if (char_supply3[charIindex3] == true)
				{
					player3_items--;
					char_supply3[charIindex3] = false;
					supply_matrix[charIndex3][2] = false;
				}
				
				else if (char_supply3[charIindex3] == false && player3_items < 3)
				{
					player3_items++;
					char_supply3[charIindex3] = true;
					supply_matrix[charIndex3][2] = true;
				}
				
			}
			//player 4 item select:
			if (Input.GetAxisRaw("360_LAX4") == -1) 
			{
				charIindex4 = menuSelection2(charIindex4, "left");
			}
			
			if (Input.GetAxisRaw("360_LAX4") == 1) 
			{
				charIindex4 = menuSelection2(charIindex4, "right");
			}
			
			if (Input.GetButtonDown("360_AButton4"))
			{
				if (char_supply4[charIindex4] == true)
				{
					player4_items--;
					char_supply4[charIindex4] = false;
					supply_matrix[charIndex4][3] = false;
				}
				
				else if (char_supply4[charIindex4] == false && player4_items < 3)
				{
					player4_items++;
					char_supply4[charIindex4] = true;
					supply_matrix[charIndex4][3] = true;
				}
				
			}
		}
	}

	void handleselection ()
	{
		GUI.FocusControl (menuOptions [selectedIndex]);


		switch(selectedIndex)
		{
		case 0:
			main = false;
			start = true;
			break;
		case 1:
			break;
		case 2:
			Application.Quit();
			break;



		}
	}



	void OnGUI () {
		if (main == true) 
		{
						// Make a background box
						GUI.Box (new Rect ((Screen.width / 2) - 50, (Screen.height / 2) - 30, 100, 150), "Loader Menu");
						float x = (Screen.width / 2) - 50;
						float y = (Screen.height / 2) - 45;

						GUI.SetNextControlName ("Start Game");
						// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
						if (GUI.Button (new Rect (x + 10, y + 40, 80, 20), "Start")) {
								main = false;
								start = true;
						}

						GUI.SetNextControlName ("Options");
						// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
						if (GUI.Button (new Rect (x + 10, y + 70, 80, 20), "Options")) {
								Application.LoadLevel (1);
						}

						/*GUI.SetNextControlName ("Credits");
						// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
						if (GUI.Button (new Rect (x + 10, y + 100, 80, 20), "Credits")) {
								Application.LoadLevel (1);
						}*/

						GUI.SetNextControlName ("Exit");
						// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
						if (GUI.Button (new Rect (x + 10, y + 130, 80, 20), "Exit")) {
								Application.LoadLevel (1);
						}

						GUI.FocusControl (menuOptions [selectedIndex]);
						//print (selectedIndex);
		}
		else if (start == true)
		{
			// Make a background box
			GUI.DrawTexture (new Rect(0,0,Screen.width,Screen.height), char_back);

			//155, 390, 625, 860
			GUI.Label (new Rect (155 + (charIndex * 235), 80, 50, 50), player1symbol);
			GUI.Label (new Rect (5, 600, 500, 500), char_des[charIndex], myStyle );
			if(player2 == true)
			{
				GUI.Label (new Rect(205 + (charIndex2 * 235) , 80, 50, 50), player2symbol);
				GUI.Label (new Rect (240, 600, 500, 500), char_des[charIndex2], myStyle );
			}

			if (player3 == true)
			{
				GUI.Label (new Rect(255 + (charIndex3 * 235), 80, 50, 50), player3symbol);
				GUI.Label (new Rect (475, 600, 500, 500), char_des[charIndex3], myStyle );
			}

			if (player4 == true)
			{
				GUI.Label (new Rect(305 + (charIndex4 * 235), 80, 50, 50), player4symbol);
				GUI.Label (new Rect (710, 600, 500, 500), char_des[charIndex4], myStyle );
			}


		}
		else if (item == true)
		{
			GUI.DrawTexture ( new Rect (0 , 0, Screen.width, Screen.height), item_back);


			/*for (int i = 0; i < 6; i++)
			{
				GUI.Label (new Rect ((50 + (10*i)) + ((Screen.width -150) * i/6),100,(Screen.width - 150)/6, 500), itempics[i]);
			
			}*/


			for (int j = 0; j < 12; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					if (supply_matrix[j][k] == true)
					{
						GUI.Label (new Rect ( 165 + ((k % 2) * 100) + (155 * (j % 6))   , 45 + (100 * (k / 2)) + (155 * (j / 6)), 40, 40), charnums[k]);
					}

				}
			}

			//165 320 475 630 785 940   (45, 200)
			//265 420 575 730 885 1040  (45, 200)
			//165 320 475 630 785 940   (145,300)
			//265 420 575 730 885 1040  (145, 300)
			GUI.Label (new Rect (165 + (155 * (charIindex % 6)), 45 + (155 * (charIindex / 6)) , 40, 40), player1symbol);
			if (player2 == true)
				GUI.Label (new Rect (265 + (155 * (charIindex2 % 6)), 45 + (155 * (charIindex2 / 6)) , 40, 40), player2symbol);
			if (player3 == true)
				GUI.Label (new Rect (165 + (155 * (charIindex3 % 6)), 145 + (155 * (charIindex3 / 6)) , 40, 40), player3symbol);
			if (player3 == true)
				GUI.Label (new Rect (265 + (155 * (charIindex4 % 6)), 145 + (155 * (charIindex4 / 6)) , 40, 40), player4symbol);
			
			

		}
	}

	void begin()
	{
		int [] classes = new int[] {charIndex,charIndex2,charIndex3,charIndex4};
		holder.player_num = player_count;
		holder.supply_matrix_hold = supply_matrix;
		holder.char_select = classes;
		Application.LoadLevel ("Second_Scene");
	}
}