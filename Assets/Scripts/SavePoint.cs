using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Collider), typeof(NetworkIdentity))]
    public class SavePoint : NetworkBehaviour, ISaveable
    {
        [SerializeField] private Collider saveTriggerreaCollider;
        [SerializeField] private int savePointIndex = 0;
        [SerializeField] private GameObject characterSeclectSecuencerPrefab;

        private static Dictionary<int, SavePoint> allSavePoints;
        private static int highestReachedSavePointIndex;

        private new Collider collider;

        private static SavePoint GetHighestReachedSpawnPoint()
        {
            if (allSavePoints.ContainsKey(highestReachedSavePointIndex))
            {
                return allSavePoints[highestReachedSavePointIndex];
            }
            else
            {
                Debug.LogError($"Highest Savepoint is set to {highestReachedSavePointIndex}, but no Savepoint in the scene is assigned that index... Might be problem with save loading");
                return null;
            }
        }

        private void Awake()
        {
            SaveManager.Instance.RegisterSavable("SavePointIndex", this);
        }

        private void Start()
        {
            collider = GetComponent<Collider>();
            if (savePointIndex < highestReachedSavePointIndex)
            {
                KillCollider();
            }
            else if (NetworkManager.singleton.IsMultiplayer && isServer)
            {
                GameObject instance = Instantiate(characterSeclectSecuencerPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(instance);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (savePointIndex > highestReachedSavePointIndex)
            {
                highestReachedSavePointIndex = savePointIndex;
            }
        }

        private void KillCollider()
        {
            Destroy(collider);
        }

        public object CollectData()
        {
            return highestReachedSavePointIndex;
        }

        public void RestoreState(object dataObject)
        {
            highestReachedSavePointIndex = (int)dataObject;
        }
    }
}
