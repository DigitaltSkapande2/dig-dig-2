using System;
using System.Threading.Tasks;
using DigDig2.CinemaCamera;
using DigDig2.CinemaCamera.CameraEffectors;
using DigDig2.Debugging;
using DigDig2.EffectSystem;
using DigDig2.Game;
using DigDig2.Player;
using UnityEngine;
using UnityEngine.Events;

namespace DigDig2.SaveSystem
{

    public class SavePoint : MonoBehaviour, IOrderedPropSaveable
    {
        [SerializeField] private GameObject characterSeclectSecuencerPrefab;
        [SerializeField] private LockTargetEffector lockTargetEffector;
        [SerializeField] private Transform singlePlayerSpawnPoint;
        [SerializeField] public float cameraYRotation;
        [Header("Events")]
        [SerializeField] public UnityEvent savePointReached = new();
        [SerializeField] public UnityEvent startSequenceDone = new();
        [SerializeField] private EffectPlayer onReachedEffect;
        [SerializeField] private float timeUntilReleaseCamera = 2;
        private new Collider collider;

        private OrderedPropSaver saver;

        private void Awake()
        {
            collider = GetComponent<Collider>();
        }
        
        public void PlayMultiplayerStartSequence()
        {
            lockTargetEffector.enabled = true;
            BetterDebug.Log("Spawning character select sequencer for multiplayer spawn...");
            CharacterSelectSequencer instance = Instantiate(characterSeclectSecuencerPrefab, transform.position, transform.rotation).GetComponent<CharacterSelectSequencer>();
            
            instance.gameStartedEvent.AddListener(OnMultiplayerCharacterSelectDone);
        }

        private void OnMultiplayerCharacterSelectDone()
        {
            lockTargetEffector.IsActive = false;
            startSequenceDone.Invoke();
            GameManager.Instance.StartGame();
        }

        public async void PlaySingleplayerStartSequence()
        {
            lockTargetEffector.enabled = true;
            BetterDebug.Log("Initializing singleplayer spawn...");
            GameManager.Instance.InitializeSingleplayerCharacter(singlePlayerSpawnPoint.position, singlePlayerSpawnPoint.rotation);

            await Task.Delay((int)(timeUntilReleaseCamera * 1000));
            lockTargetEffector.IsActive = false;
            startSequenceDone.Invoke();
            GameManager.Instance.StartGame();
        }

        public void SetSpawnPointReached(bool reached)
        {
            collider.enabled = !reached;
            BetterDebug.Log($"i am active, {name}");

            if (reached)
            {
                savePointReached.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            BetterDebug.Log("reached: " + gameObject.name);

            SetSpawnPointReached(true);
            onReachedEffect?.Play();

            if (SaveManager.Instance.isMultiplayer)
            {
                foreach (PlayerController player in GameManager.Instance.playerControllers)
                {
                    if (!player.IsAlive) player.ReSpawnCharacter(singlePlayerSpawnPoint.transform.position);
                }
            }
            
            SaveManager.Instance.SaveAllAndWriteToFile();
        }
        
        public void OnLoaded(int myId, int activeId, OrderedPropSaver propSaver)
        {
            SetSpawnPointReached(myId <= activeId);
            if (myId > activeId)
            {
                saver = propSaver;
                savePointReached.AddListener(IncrementSaverActiveIndex);
            }

            if (myId == activeId)
            {
                GameCamera.Instance.SetTargetRotation(cameraYRotation);
                if (GameManager.Instance.IsMultiplayer)
                {
                    PlayMultiplayerStartSequence();
                }
                else
                {
                    PlaySingleplayerStartSequence();
                }
            }
        }

        private void IncrementSaverActiveIndex()
        {
            saver.IncrementActiveIndex();
        }
    }
}
