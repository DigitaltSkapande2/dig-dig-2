using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.Serialization;

using Random = UnityEngine.Random;

namespace DigDig2.EffectSystem.Footsteps {
	[Serializable]
	public struct FootstepMaterialOverride {
		public Material material;

		[FormerlySerializedAs( "footstepSFXPrefab" )]
		public List<GameObject> footstepSfxPrefab;
	}

	public class FootstepSfxPlayer : MonoBehaviour {
		private const bool IS_ACTIVE = true;

		[FormerlySerializedAs( "EffectPrefab" )] [SerializeField]
		private GameObject[ ] effectPrefab;

		[Header( "Sense Overrides" )]
		[SerializeField] private bool overrideSense = true;

		[SerializeField] private LayerMask overrideSenseLayer;
		[SerializeField] private Vector3 overrideSenseRaycastVector = Vector3.down;
		[SerializeField] private FootstepMaterialOverride[ ] footstepMaterialOverrides;

		// erm this is not used :3 should it be?
		public void OnFootStepEvent( ) {
			if ( !IS_ACTIVE ) return;

			GameObject effectToPlay = effectPrefab[ Random.Range( 0, effectPrefab.Length ) ];

			if ( overrideSense ) {
				if ( Physics.Raycast(
					transform.position,
					overrideSenseRaycastVector,
					out RaycastHit hit,
					overrideSenseRaycastVector.magnitude,
					overrideSenseLayer
				) ) {
					if ( TryGetMaterialFromRaycastHit( hit, out Material material ) ) {
						foreach ( FootstepMaterialOverride footstepMaterialOverride in footstepMaterialOverrides ) {
							if ( footstepMaterialOverride.material != material ) continue;

							effectToPlay = footstepMaterialOverride.footstepSfxPrefab[ Random.Range( 0, footstepMaterialOverride.footstepSfxPrefab.Count ) ];
							break;
						}
					}
				}
			}

			Destroy( Instantiate( effectToPlay, transform.position, Quaternion.identity ), 2f );
		}

		private static bool TryGetMaterialFromRaycastHit( RaycastHit hit, out Material material ) {
			MeshFilter meshFilter = hit.collider.GetComponent<MeshFilter>( );
			if ( !meshFilter ) {
				material = null;
				return false;
			}

			Mesh mesh = meshFilter.sharedMesh ?? meshFilter.mesh;
			if ( !mesh ) {
				material = null;
				return false;
			}

			Renderer renderer = meshFilter.GetComponent<Renderer>( );
			if ( !renderer ) {
				material = null;
				return false;
			}

			int subMeshIndex = GetSubMeshIndex( mesh, hit.triangleIndex );

			Material[ ] materials = renderer.sharedMaterials ?? renderer.materials;

			int materialIndex;

			// If there are more materials than there are sub-meshes,
			// Unity renders the last sub-mesh with each of the remaining materials,
			// one on top of the next.
			if ( materials.Length > mesh.subMeshCount &&
				subMeshIndex == mesh.subMeshCount - 1 )
				materialIndex = materials.Length - 1;
			else
				materialIndex = subMeshIndex;

			material = materials.ElementAtOrDefault( materialIndex );
			return true;
		}

		private static int GetSubMeshIndex( Mesh mesh, int triangleIndex ) {
			for ( int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++ ) {
				int[ ] subMeshTriangles = mesh.GetTriangles( subMeshIndex );

				for ( int i = 0; i < subMeshTriangles.Length; i += 3 ) {
					if ( subMeshTriangles[ i ] == mesh.triangles[ triangleIndex * 3 ] &&
						subMeshTriangles[ i + 1 ] == mesh.triangles[ triangleIndex * 3 + 1 ] &&
						subMeshTriangles[ i + 2 ] == mesh.triangles[ triangleIndex * 3 + 2 ] )
						return subMeshIndex;
				}
			}

			return -1;
		}
	}
}
