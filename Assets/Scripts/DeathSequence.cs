using DigDig2.Effects;

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
            deathEffectPlayer.Play(transform.position, Quaternion.identity, Vector3.one);

            if (GameManager.Instance.IsMultiplayer)
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
                        mat.SetFloat(disolveFloatName, newDissolveAmount);
                    }
                }
            }
        }

        private void SingleplayerResetScene()
        {
            GameManager.Instance.ReloadGameScene();
        }
    }
}
