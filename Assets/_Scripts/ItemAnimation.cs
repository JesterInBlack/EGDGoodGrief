using UnityEngine;
using System.Collections;

public class ItemAnimation : MonoBehaviour 
{
	//A class to display an item's icon over the player using it.

	#region vars
	private SpriteRenderer mySpriteRenderer;
	private Player player;
	private ItemName prevName;
	#endregion

	// Use this for initialization
	void Start () 
	{
		mySpriteRenderer = GetComponent<SpriteRenderer>(); //amortize getcomponent cost
		player = this.gameObject.transform.parent.GetComponent<Player>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if ( prevName != player.items[ player.itemIndex ].name ) //changed this frame
		{
			//change image
			mySpriteRenderer.sprite = ItemImages.getSprite ( player.items[ player.itemIndex ].name );
		}
		prevName = player.items[ player.itemIndex ].name;

		//show / hide.
		if ( player.state == "item windup" || 
		     player.state == "item charge" || 
		     player.state == "item aim ray" || 
		     player.state == "item aim point" )
		{
			mySpriteRenderer.enabled = true;  //show it.
		}
		else
		{
			mySpriteRenderer.enabled = false; //hide it.
		}
	}
}
