using System;
using DigDig2.CinemaCamera;
using UnityEngine;

namespace DigDig2.Effects
{
    public class SpawnPrefabEffectInstance : ICloneable
    {
        [SerializeField] private GameObject prefabToSpawn;

        private float startTime;

        public object Clone()
        {
            SpawnPrefabEffectInstance clone = new();
            clone.prefabToSpawn = prefabToSpawn;
            clone.startTime = startTime;
            return clone;
        }
    }

    public class SpawnPrefabEffect : EffectBase<SpawnPrefabEffectInstance>
    {
        internal override void OnEffectEnd(SpawnPrefabEffectInstance effectInstance)
        {

        }


    }

}
