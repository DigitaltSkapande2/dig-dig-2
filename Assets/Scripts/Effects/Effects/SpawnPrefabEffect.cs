using System;
using System.Collections.Generic;
using UnityEngine;

namespace DigDig2.Effects
{
    [Serializable]
    public class SpawnPrefabEffectData : ICloneable
    {
        [Serializable]
        public struct PrefabToSpawn
        {
            public GameObject prefab;
            [Range(1f, 10f)] public float duration;
        }

        public List<PrefabToSpawn> prefabToSpawn;
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
                Debug.LogWarning($"SpawnPrefabEffect: Max simultaneous spawns reached ({maxSimultaneousSpawns}). Cannot spawn more prefabs until some are destroyed.");
                return;
            }

            foreach (var prefabToSpawn in effectInstance.prefabToSpawn)
            {
                Debug.Log($"Spawning prefab {prefabToSpawn.prefab.name} at {effectInstance.position} with rotation {effectInstance.rotation} and scale {effectInstance.scale} for duration {prefabToSpawn.duration}");
                SpawnPrefab(prefabToSpawn.prefab, effectInstance.position, effectInstance.rotation, effectInstance.scale, prefabToSpawn.duration);
            }
        }

        private void SpawnPrefab(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 scale, float duration)
        {
            currentlySpawnedPrefabsCount++;
            GameObject spawned = Instantiate(prefab, position, rotation);
            spawned.transform.localScale = scale;

            Destroy(spawned, duration);
            Invoke(nameof(OnSpawnedPrefabDestroyed), duration);
        }

        private void OnSpawnedPrefabDestroyed()
        {
            currentlySpawnedPrefabsCount = Mathf.Max(0, currentlySpawnedPrefabsCount - 1);
        }

    }

}
