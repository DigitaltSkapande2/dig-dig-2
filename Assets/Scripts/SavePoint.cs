using System.Collections.Generic;
using Mirror;
using Mirror.BouncyCastle.Crypto.Parameters;
using UnityEditor.Build;
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
            collider = GetComponent<Collider>();
        }

        public override void OnStartServer()
        {
            if (NetworkManager.singleton.IsMultiplayer)
            {
                GameObject instance = Instantiate(characterSeclectSecuencerPrefab, transform.position, Quaternion.identity);
                NetworkServer.Spawn(instance);
            }

            if (savePointIndex <= highestReachedSavePointIndex)
            {
                SetSpawnPointAchieved(true);
            }
        }


        [ClientRpc]
        private void SetSpawnPointAchieved(bool achieved)
        {

            KillCollider();
        }


        private void OnTriggerEnter(Collider other)
        {
            if (isServer && savePointIndex > highestReachedSavePointIndex)
            {
                highestReachedSavePointIndex = savePointIndex;
            }

            SetSpawnPointAchieved(true);
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
