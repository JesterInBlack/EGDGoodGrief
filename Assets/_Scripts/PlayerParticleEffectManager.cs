using UnityEngine;
using System.Collections;

public class PlayerParticleEffectManager : MonoBehaviour 
{
	#region vars
	private Player player;

	public ParticleSystem healingEffect;
	public ParticleSystem poisonEffect;
	public GameObject sandstormEffect;
	#endregion

	// Use this for initialization
	void Awake () 
	{
		player = this.gameObject.GetComponent<Player>();

		if ( player.characterclass != CharacterClasses.DEFENDER )
		{
			foreach ( ParticleSystem p in sandstormEffect.GetComponentsInChildren<ParticleSystem>() )
			{
				p.enableEmission = false;
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		#region poison
		//Check if player is poisoned.
		bool isPoisoned = false;
		for ( int i = 0; i < player.buffs.Count; i++)
		{
			if ( ((Buff)player.buffs[i]).regen < 0.0f )
			{
				isPoisoned = true;
			}
		}

		if ( isPoisoned )
		{
			poisonEffect.enableEmission = true;
		}
		else
		{
			poisonEffect.enableEmission = false;
		}
		#endregion

		#region healing
		if ( player.isActuallyHealing )
		{
			healingEffect.enableEmission = true;
		}
		else
		{
			healingEffect.enableEmission = false;
		}
		#endregion

		#region sandstorm
		if ( player.characterclass == CharacterClasses.DEFENDER )
		{
			foreach ( ParticleSystem p in sandstormEffect.GetComponentsInChildren<ParticleSystem>() )
			{
				if ( player.state == "sandstorm" )
				{
					p.enableEmission = true;
				}
				else
				{
					p.enableEmission = false;
				}
			}
		}
		#endregion
	}
}
