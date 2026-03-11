using System;
using System.Collections.Generic;

using UnityEngine;

namespace DigDig2.EffectSystem.Effects {
	[Serializable]
	public class SpawnPrefabEffectData : ICloneable {
		public List<PrefabToSpawn> prefabToSpawn;
		[NonSerialized] public Transform parent;
		[NonSerialized] public Vector3 position;
		[NonSerialized] public Quaternion rotation;
		[NonSerialized] public Vector3 scale = Vector3.one;

		public object Clone( ) => MemberwiseClone( );

		[Serializable]
		public struct PrefabToSpawn {
			public GameObject prefab;
			public bool hasLifetime;
			[Range( 0f, 10f )] public float lifetime;
		}
	}

	public class SpawnPrefabEffect : Effect<SpawnPrefabEffectData> {
		[SerializeField] private int maxSimultaneousSpawns = 200;
		private int currentlySpawnedPrefabsCount;

		public override void PlayEffectInstance( SpawnPrefabEffectData effectInstance ) {
			if ( currentlySpawnedPrefabsCount >= maxSimultaneousSpawns ) {
				Debug.LogWarning( $"SpawnPrefabEffect: Max simultaneous spawns reached ({maxSimultaneousSpawns}). Cannot spawn more prefabs until some are destroyed." );
				return;
			}

			foreach ( SpawnPrefabEffectData.PrefabToSpawn prefabToSpawn in effectInstance.prefabToSpawn ) {
				if ( prefabToSpawn.hasLifetime ) currentlySpawnedPrefabsCount++;
				if ( !prefabToSpawn.prefab ) {
					Debug.LogError( "Cannot insantiate null prefab!" );
					return;
				}

				GameObject spawned = Instantiate( prefabToSpawn.prefab, effectInstance.position, effectInstance.rotation, effectInstance.parent );
				spawned.transform.localScale = effectInstance.scale;

				if ( prefabToSpawn.hasLifetime ) Destroy( spawned, prefabToSpawn.lifetime );
				if ( prefabToSpawn.hasLifetime ) Invoke( nameof( OnSpawnedPrefabDestroyed ), prefabToSpawn.lifetime );
			}
		}

		private void OnSpawnedPrefabDestroyed( ) { currentlySpawnedPrefabsCount = Mathf.Max( 0, currentlySpawnedPrefabsCount - 1 ); }
	}
}
