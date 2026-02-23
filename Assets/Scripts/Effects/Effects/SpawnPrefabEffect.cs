using DigDig2.CinemaCamera;
using UnityEngine;

namespace DigDig2.Effects
{
    public class SpawnPrefabEffectInstance
    {
        [SerializeField] private GameObject prefabToSpawn;

        private float startTime;
    }

    public class SpawnPrefabEffect : EffectBase<SpawnPrefabEffectInstance>
    {



    }

}
