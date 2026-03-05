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
        [SerializeField] public float cameraYRotation;
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
            GameManager.Instance.InitializeSingleplayerCharacter(singlePlayerSpawnPoint.position, Quaternion.identity);

            await Task.Delay((int)(timeUntilReleaseCamera * 1000));
            lockTargetEffector.IsActive = false;
        }

        [Server]
        public void ServerSetSpawnPointReached(bool reached)
        {
            RcpSetSpawnPointReached(reached);
        }


        public void RcpSetSpawnPointReached(bool reached)
        {
            collider.enabled = !reached;
            VerboseLog($"i am active, {name}");

            if (reached)
            {
                onReachedEffect.Play();
                savePointReached.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isServer) return;
            VerboseLog("reached: " + gameObject.name);

            RcpSetSpawnPointReached(true);
            SaveManager.Instance.SaveAllAndWriteToFile();
        }

        private void VerboseLog(string message)
        {
            Debug.Log("SAVEPOINT: " + message);
        }
    }
}
