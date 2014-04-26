using UnityEngine;
using System.Collections;

public class ScoreText : MonoBehaviour 
{
	#region vars
	public TextMesh scoreName;
	public TextMesh scorePoints;

	private float t = 0.0f;
	private float speed = 1.0f;
	private float acceleration = -1.0f;

	private Vector2 randomFactor;
	#endregion

	// Use this for initialization
	void Start () 
	{
		scoreName.renderer.sortingLayerName = "Projectile";
		scorePoints.renderer.sortingLayerName = "Projectile";
		randomFactor = new Vector2( Random.Range ( -1.0f, 1.0f ), Random.Range ( -1.0f, 1.0f ) );
		randomFactor = randomFactor * 0.5f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		scoreName.color = new Color( scoreName.color.r, scoreName.color.g, scoreName.color.b, 1.0f - t );
		scorePoints.color = new Color( scorePoints.color.r, scorePoints.color.g, scorePoints.color.b, 1.0f - t );
		float dt = Time.deltaTime * StaticData.t_scale;
		transform.position += new Vector3( 0.0f, speed * dt, 0.0f );
		transform.position += new Vector3( randomFactor.x * dt, randomFactor.y * dt, 0.0f );

		speed += acceleration * Time.deltaTime * StaticData.t_scale;
		t += Time.deltaTime * StaticData.t_scale / 2.0f;
		if ( t >= 1.0f )
		{
			Destroy ( this.gameObject );
		}
	}
}
