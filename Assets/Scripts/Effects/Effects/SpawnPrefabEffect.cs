using System;
using DigDig2.CinemaCamera;
using UnityEngine;

namespace DigDig2.Effects
{
    public class SpawnPrefabEffectData : ICloneable
    {
        public GameObject prefabToSpawn;
        public float duration = 2f;
        [NonSerialized] public Vector3 position;
        [NonSerialized] public Quaternion rotation;
        [NonSerialized] public Vector3 scale = Vector3.one;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public class SpawnPrefabEffect : EffectBase<SpawnPrefabEffectData>
    {
        [SerializeField] private int maxSimultaneousSpawns = 200;
        int currentlySpawnedPrefabsCount = 0;

        public override void PlayEffectInstance(SpawnPrefabEffectData effectInstance)
        {
            if (currentlySpawnedPrefabsCount >= maxSimultaneousSpawns)
            {
                return;
            }

            currentlySpawnedPrefabsCount++;
            GameObject spawned = Instantiate(effectInstance.prefabToSpawn, effectInstance.position, effectInstance.rotation);
            spawned.transform.localScale = effectInstance.scale;

            Destroy(spawned, effectInstance.duration);
            Invoke(nameof(OnSpawnedPrefabDestroyed), effectInstance.duration);
        }

        private void OnSpawnedPrefabDestroyed()
        {
            currentlySpawnedPrefabsCount = Mathf.Max(0, currentlySpawnedPrefabsCount - 1);
        }

    }

}
