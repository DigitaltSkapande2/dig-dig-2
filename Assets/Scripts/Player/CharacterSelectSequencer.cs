using System.Threading.Tasks;

using DigDig2.Game;
using DigDig2.Player.Combat;

using TMPro;

using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace DigDig2.Player {
	public class CharacterSelectSequencer : MonoBehaviour {
		private void OnSwitchCharacterButtonClicked( ) { PlayerOneIsMax = !PlayerOneIsMax; }

		private async void OnStartButtonClicked( ) {
			Destroy( maxDummyInstance );
			Destroy( minisDummyInstance );

			GameObject maxInstance = Instantiate( maxPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation );
			GameObject minisInstance = Instantiate( minisPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation );

			GameObject hostCharObjInstance = PlayerOneIsMax ? maxInstance : minisInstance;
			GameObject clientCharObjInstance = PlayerOneIsMax ? minisInstance : maxInstance;

			// hostCharObjInstance.name = PlayerOneIsMax ? "Max_connid: " + hostConn.connectionId : "Minis_connid: " + hostConn.connectionId;
			// clientCharObjInstance.name = PlayerOneIsMax ? "Minis_connid: " + clientConn.connectionId : "Max_connid: " + clientConn.connectionId;

			// NetworkServer.RemovePlayerForConnection(hostConn, RemovePlayerOptions.Destroy);
			// NetworkServer.RemovePlayerForConnection(clientConn, RemovePlayerOptions.Destroy);

			await Task.Delay( 100 ); // wait a frame for the ReplacePlayerForConnection to complete before enabling input

			EnablePlayerInput( );
		}

		private void EnablePlayerInput( ) {
			GameManager.Instance.PlayerOneCharacter.GetComponent<PlayerCharacterInput>( ).EnableInput( );
			GameManager.Instance.PlayerOneCharacter.GetComponent<PlayerAttackInput>( ).EnableInput( );
			GameManager.Instance.PlayerOneCharacter.GetComponent<EntityCharacterController>( ).Frozen = false;
		}

		#region Util

		private void VerboseLog( string msg ) {
			if ( verboseLogging ) Debug.Log( "CHARACTER_SELECT: " + msg );
		}

		#endregion

		#region Variables

		[Header( "Prefabs" )]
		[SerializeField] private GameObject maxPrefab;

		[SerializeField] private GameObject minisPrefab;
		[SerializeField] private GameObject maxDummyPrefab;
		[SerializeField] private GameObject minisDummyPrefab;

		[Header( "Scene Refs" )]
		[SerializeField] private Transform maxSpawnPoint;

		[SerializeField] private Transform minisSpawnPoint;

		[Header( "UI Refs" )]
		[SerializeField] private Button switchCharacterButton;

		[SerializeField] private Button startButton;
		[SerializeField] private TMP_Text maxNameplateText;
		[SerializeField] private TMP_Text minisNameplateText;

		[Header( "Debug" )]
		[SerializeField] private bool verboseLogging;

		[FormerlySerializedAs( "hostIsMax" )]
		[SerializeField] private bool PlayerOneIsMax = true;

		private GameObject maxDummyInstance;
		private GameObject minisDummyInstance;

		#endregion

		#region UnityCallbacks

		private void Awake( ) {
			if ( GameManager.Instance.IsMultiplayer ) return;

			VerboseLog( "Not in multiplayer mode, disabling character select sequencer." );
			gameObject.SetActive( false );
			Destroy( gameObject );
		}

		private void Start( ) {
			// maxClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Max, maxClickableCollider));
			// minisClickableCollider.clickStart.AddListener(() => OnCharacterClicked(CharacterType.Mini, minisClickableCollider));

			switchCharacterButton.interactable = false;
			startButton.interactable = false;

			maxDummyInstance = Instantiate( maxDummyPrefab, maxSpawnPoint.position, maxSpawnPoint.rotation );
			minisDummyInstance = Instantiate( minisDummyPrefab, minisSpawnPoint.position, minisSpawnPoint.rotation );

			switchCharacterButton.interactable = true;
			startButton.interactable = true;

			switchCharacterButton.onClick.AddListener( OnSwitchCharacterButtonClicked );
			startButton.onClick.AddListener( OnStartButtonClicked );

			VerboseLog( "Server ready. Waiting for clients to connect before spawning characters." );
		}

		#endregion
	}
}
