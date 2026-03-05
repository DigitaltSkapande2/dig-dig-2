using DigDig2.Effects;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Splines.Interpolators;

namespace DigDig2
{
    public class DeathSequence : MonoBehaviour
    {
        [SerializeField] float timeUntilRespawn = 2f;
        [SerializeField] GameObject emptyPlayerPrefab; 
        [SerializeField] private EffectPlayer deathEffectPlayer;
        [SerializeField] private float disolveWeight = 3f;
        [SerializeField] private SkinnedMeshRenderer[] targetMeshRenderer;
        [SerializeField] private string disolveFloatName = "DissolveAmount";


        bool isDying;

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
                isDying = true;
                Invoke(nameof(SingleplayerResetScene), timeUntilRespawn);
            }
        }

        void Update()
        {
            if (isDying)
            {
                foreach (SkinnedMeshRenderer renderer in targetMeshRenderer)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        float newDissolveAmount = Mathf.Lerp(mat.GetFloat(disolveFloatName), 1f, disolveWeight*Time.deltaTime);
                        Debug.Log("newDissolveAmount: " + newDissolveAmount);
                        mat.SetFloat(disolveFloatName, newDissolveAmount);
                    }
                }
            }
        }

        private void SingleplayerResetScene()
        {
            var playerInstance = Instantiate(emptyPlayerPrefab);
            DontDestroyOnLoad(playerInstance);
            NetworkServer.ReplacePlayerForConnection(NetworkServer.localConnection, playerInstance, ReplacePlayerOptions.Destroy);
            Debug.Log(SceneManager.GetActiveScene().name);
            NetworkManager.singleton.ServerChangeScene(SceneManager.GetActiveScene().name);
        }


    }
}
