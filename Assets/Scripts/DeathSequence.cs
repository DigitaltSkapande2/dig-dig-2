using DigDig2.Effects;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DigDig2
{
    public class DeathSequence : MonoBehaviour
    {
        [SerializeField] float timeUntilRespawn = 2f;
        [SerializeField] GameObject emptyPlayerPrefab; 
        [SerializeField] private EffectPlayer deathEffectPlayer;
        public void StartDeathSequence()
        {
            if (!NetworkServer.active) return;

            deathEffectPlayer.Play(transform.position, Quaternion.identity, Vector3.one);

            if (NetworkManager.singleton.IsMultiplayer)
            {
                // TODO: Implement multiplayer respawn system
            }
            else
            {
                Invoke(nameof(SingleplayerResetScene), timeUntilRespawn);
            }
        }

        private void SingleplayerResetScene()
        {
            Destroy(GameManager.Instance.gameObject);
            Destroy(EffectCore.Instance.gameObject);
            var playerInstance = Instantiate(emptyPlayerPrefab);
            DontDestroyOnLoad(playerInstance);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, playerInstance, ReplacePlayerOptions.Destroy);
            Debug.Log(SceneManager.GetActiveScene().name);
            NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
        }
    }
}
