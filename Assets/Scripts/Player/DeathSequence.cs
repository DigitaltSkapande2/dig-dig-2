using DigDig2.EffectSystem;
using DigDig2.Game;

using UnityEngine;
using UnityEngine.Serialization;

namespace DigDig2.Player {
	public class DeathSequence : MonoBehaviour {
		[SerializeField] private float timeUntilRespawn = 2f;
		[SerializeField] private GameObject emptyPlayerPrefab;
		[SerializeField] private EffectPlayer deathEffectPlayer;
		[SerializeField] private float disolveWeight = 3f;

		[FormerlySerializedAs( "targetMeshRenderer" )] [SerializeField]
		private SkinnedMeshRenderer[ ] targetMeshRenderers;

		[SerializeField] private string disolveFloatName = "DissolveAmount";

		private bool isDying;

		private void Update( ) {
			if ( !isDying ) return;

			foreach ( SkinnedMeshRenderer targetMeshRenderer in targetMeshRenderers ) {
				foreach ( Material mat in targetMeshRenderer.materials ) {
					float newDissolveAmount = Mathf.Lerp( mat.GetFloat( disolveFloatName ), 1f, disolveWeight * Time.deltaTime );
					mat.SetFloat( disolveFloatName, newDissolveAmount );
				}
			}
		}

		public void StartDeathSequence( ) {
			deathEffectPlayer.Play( transform.position, Quaternion.identity, Vector3.one );

			if ( GameManager.Instance.IsMultiplayer ) {
				// TODO: Implement multiplayer respawn system
			} else {
				isDying = true;
				Invoke( nameof( SingleplayerResetScene ), timeUntilRespawn );
			}
		}

		private void SingleplayerResetScene( ) { GameManager.Instance.ReloadGameScene( ); }
	}
}
