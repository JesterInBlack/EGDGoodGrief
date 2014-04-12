using UnityEngine;
using System.Collections;

public class Info_Script : MonoBehaviour {


	public int player_num;

	public bool[][] supply_matrix_hold;

	public int[] char_select;

	// Use this for initialization
	void Awake(){

		DontDestroyOnLoad (transform.gameObject);
	}


}
