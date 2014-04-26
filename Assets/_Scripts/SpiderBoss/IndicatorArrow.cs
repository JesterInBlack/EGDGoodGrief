using UnityEngine;
using System.Collections;

public class IndicatorArrow : MonoBehaviour 
{
	public GameObject _arrow;
	public GameObject _text;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	public void Enable()
	{
		GetComponent<AudioSource>().PlayOneShot ( SoundStorage.BossIndicator, 0.6f );
		_arrow.SetActive(true);
		_text.SetActive(true);
	}

	public void Disable()
	{
		_arrow.SetActive(false);
		_text.SetActive(false);
	}
}
