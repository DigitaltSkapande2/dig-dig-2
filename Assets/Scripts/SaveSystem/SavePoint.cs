using System.Threading.Tasks;
using DigDig2.CinemaCamera.CameraEffectors;
using DigDig2.EffectSystem;
using DigDig2.Game;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.SaveSystem
{

    public class SavePoint : MonoBehaviour
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
        
        public void ServerStartMultiplayerStartSequence()
        {
            lockTargetEffector.IsActive = true;
            VerboseLog("Spawning character select sequencer for multiplayer spawn...");
            CharacterSelectSequencer instance = Instantiate(characterSeclectSecuencerPrefab, transform.position, Quaternion.identity).GetComponent<CharacterSelectSequencer>();
            
            instance.gameStartedEvent.AddListener(() => { lockTargetEffector.IsActive = false; });
        }

        public async void ServerStartSingleplayerStartSequence()
        {
            lockTargetEffector.IsActive = true;
            VerboseLog("Initializing singleplayer spawn...");
            GameManager.Instance.InitializeSingleplayerCharacter(singlePlayerSpawnPoint.position, Quaternion.identity);

            await Task.Delay((int)(timeUntilReleaseCamera * 1000));
            lockTargetEffector.IsActive = false;
        }

        public void SetSpawnPointReached(bool reached)
        {
            collider.enabled = !reached;
            VerboseLog($"i am active, {name}");

            if (reached)
            {
                savePointReached.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            VerboseLog("reached: " + gameObject.name);

            SetSpawnPointReached(true);
            onReachedEffect.Play();
            SaveManager.Instance.SaveAllAndWriteToFile();
        }

        private void VerboseLog(string message)
        {
            Debug.Log("SAVEPOINT: " + message);
        }
    }
}
