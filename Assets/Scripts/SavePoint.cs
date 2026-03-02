using System.Threading.Tasks;
using DigDig2.CinemaCamera;
using DigDig2.Effects;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2
{
    [RequireComponent(typeof(Collider), typeof(NetworkIdentity))]
    public class SavePoint : NetworkBehaviour
    {
        [SerializeField] private GameObject characterSeclectSecuencerPrefab;
        [SerializeField] private LockTargetEffector lockTargetEffector;
        [SerializeField] private Transform singlePlayerSpawnPoint;
        [Header("Events")]
        [SerializeField] public UnityEvent savePointReached;
        [SerializeField] private EffectPlayer onReachedEffect;
        [SerializeField] private float timeUntilReleaseCamera = 2;
        private new Collider collider;
        private int HighestReachedSavePointIndex { 
            get
            {
                return GameManager.Instance.loadedGameManagerSaveData.highestReachedSavePointIndex;
            } 
            set
            {
                GameManager.Instance.SetHighestReachedSavePointIndex(value);
            } 
        }
        

        private void Awake()
        {
            collider = GetComponent<Collider>();
            lockTargetEffector.IsActive = false;
        }

        public void Start()
        {
            
        }
        
        [Server]
        public void ServerStartMultiplayerStartSequence()
        {
            lockTargetEffector.IsActive = true;
            VerboseLog("Spawning character select sequencer for multiplayer spawn...");
            GameObject instance = Instantiate(characterSeclectSecuencerPrefab, transform.position, Quaternion.identity);
            NetworkServer.Spawn(instance);
        }

        [Server]
        public async void ServerStartSingleplayerStartSequence()
        {
            lockTargetEffector.IsActive = true;
            VerboseLog("Initializing singleplayer spawn...");
            GameManager.Instance.InitializeSingleplayerCharacter(singlePlayerSpawnPoint.position, singlePlayerSpawnPoint.rotation);

            GameCamera.Instance.ZoomOutAnimation(); // TODO: MAKE THIS BETTER THAN REGULAR FUCKASS ANIMATOR ON CAMERA
            await Task.Delay((int)(timeUntilReleaseCamera * 1000));
            lockTargetEffector.IsActive = false;
        }

        [ClientRpc]
        private void SetSpawnPointReached(bool reached)
        {
            collider.enabled = !reached; 

            if (reached)
            {
                onReachedEffect.Play();
                savePointReached.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;

            savePointReached.Invoke();
            SetSpawnPointReached(true);
        }

        private void VerboseLog(string message)
        {
            Debug.Log("SAVEPOINT: " + message);
        }
    }
}
