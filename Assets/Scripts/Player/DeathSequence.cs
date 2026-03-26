using DigDig2.EffectSystem;
using DigDig2.Game;

using UnityEngine;
using UnityEngine.Serialization;

namespace DigDig2.Player
{
	public class DeathSequence : MonoBehaviour
	{
		[SerializeField] private float timeUntilRespawn = 2f;
		[SerializeField] private GameObject emptyPlayerPrefab;
		[SerializeField] private EffectPlayer deathEffectPlayer;
        [SerializeField] private EffectPlayer multiplayerDeathEffectPlayer;
		[SerializeField] private float disolveWeight = 3f;

        [Header("Multiplayer")]
        [SerializeField] private EntityCharacterController entityToFreeze;
        [SerializeField] private GameObject characterObjectRef;

		[FormerlySerializedAs( "targetMeshRenderer" )] [SerializeField]
		private SkinnedMeshRenderer[ ] targetMeshRenderers;

		[SerializeField] private string disolveFloatName = "DissolveAmount";

		private bool isDying;

		private void Update( )
		{
			if ( !isDying ) return;

			foreach ( SkinnedMeshRenderer targetMeshRenderer in targetMeshRenderers )
			{
				foreach ( Material mat in targetMeshRenderer.materials )
				{
					float newDissolveAmount = Mathf.Lerp( mat.GetFloat( disolveFloatName ), 1f, 1f - Mathf.Exp(-disolveWeight * Time.deltaTime) );
					mat.SetFloat( disolveFloatName, newDissolveAmount );
				}
			}
		}

		public void StartDeathSequence( )
		{
			
            entityToFreeze.Frozen = true;

			if ( GameManager.Instance.IsMultiplayer )
			{
                multiplayerDeathEffectPlayer?.Play( transform.position, Quaternion.identity, Vector3.one );
				GameManager.Instance.RegisterCharacterDeath(characterObjectRef);
                isDying = true;
            }
			else
			{
                deathEffectPlayer?.Play( transform.position, Quaternion.identity, Vector3.one );
				isDying = true;
				Invoke( nameof( SingleplayerResetScene ), timeUntilRespawn );
			}
		}

		private void SingleplayerResetScene( ) { GameManager.Instance.ReloadGameScene( ); }
	}
}
