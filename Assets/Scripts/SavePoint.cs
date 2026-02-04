using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DigDig2
{
    [RequireComponent(typeof(Collider))]
    public class SavePoint : MonoBehaviour, ISaveable
    {
        [SerializeField] Collider saveTriggerreaCollider;
        [SerializeField] int savePointIndex = 0;

        private static Dictionary<int, SavePoint> allSavePoints;
        private static int highestReachedSavePointIndex;

        private Collider collider;

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

        void Awake()
        {
            SaveService.Instance.RegisterSavable("SavePointIndex", this);
        }

        void Start()
        {
            collider = GetComponent<Collider>();
            if (savePointIndex < highestReachedSavePointIndex)
            {
                KillCollider();
            }
        }


        void OnTriggerEnter(Collider other)
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
