using System;
using System.Collections.Generic;

using DigDig2.Combat;

using Unity.Mathematics;

using UnityEngine;

namespace DigDig2.Game {
	[RequireComponent( typeof( Attackable ), typeof( Health ) )]
	public class Crystal : MonoBehaviour {
		private const float NO_CRACK_STAGE = 0.2f;
		private const float LIGHT_CRACK_STAGE = 0.015f;
		private const float HEAVY_CRACK_STAGE = 0.001f;
		private static readonly int crackStage = Shader.PropertyToID( "_CrackStage" );

		[Tooltip( "Enemies connected to the crystal." )]
		[SerializeField] private List<EnemyConnections> enemyConnections = new( );

		[Tooltip( "The crystal visual GameObject." )]
		[SerializeField] private GameObject crystal;

		[Tooltip( "The shield visual GameObject." )]
		[SerializeField] private GameObject shield;

		[Tooltip( "The line prefab." )]
		[SerializeField] private GameObject linePrefab;

		[Tooltip( "How far the crystal bobs up and down" )]
		[SerializeField] private float bobStrength;

		[Tooltip( "How fast the crystal bobs up and down" )]
		[SerializeField] private float bobSpeed;

		private Attackable attackable;
		private MeshRenderer crystalMeshRenderer;

		private bool hasShield = true;
		private Health health;

		private Vector3 originalPos;

		private void Awake( ) {
			attackable = GetComponent<Attackable>( );
			health = GetComponent<Health>( );

			attackable.hit.AddListener( OnHit );
		}

		private void Start( ) {
			if ( crystal == null ) {
				Debug.LogError( "Crystal has not been assigned." );
				return;
			}

			crystalMeshRenderer = crystal.GetComponent<MeshRenderer>( );

			Material material = crystalMeshRenderer.material;
			material.SetFloat( crackStage, NO_CRACK_STAGE );

			originalPos = transform.position;
		}

		private void Update( ) {
			HeightBob( );

			shield.SetActive( hasShield );
			health.enabled = !hasShield;

			for ( int index = enemyConnections.Count - 1; index >= 0; index-- ) {
				EnemyConnections enemyConnection = enemyConnections[ index ];
				if ( !enemyConnection.lineDrawer && !enemyConnection.enemy ) continue;

				if ( !enemyConnection.lineDrawer ) {
					enemyConnection.lineDrawer = Instantiate( linePrefab, transform.position, quaternion.identity, transform );
					enemyConnection.lineComponent = enemyConnection.lineDrawer.GetComponent<CrystalLine>( );
					enemyConnections[ index ] = enemyConnection;
				}

				if ( !enemyConnection.enemy ) {
					Destroy( enemyConnection.lineDrawer );
					enemyConnections.RemoveAt( index );

					continue;
				}

				enemyConnection.lineComponent.SetPositions( transform.position, enemyConnection.enemy.transform.position );
			}

			if ( enemyConnections.Count <= 0 ) hasShield = false;
		}

		private void OnHit( ) {
			if ( hasShield ) return;

			Material material = crystalMeshRenderer.material;

			switch ( health.HealthPoints ) {
				case 2: material.SetFloat( crackStage, LIGHT_CRACK_STAGE ); break;
				case 1: material.SetFloat( crackStage, HEAVY_CRACK_STAGE ); break;
			}
		}

		private void HeightBob( ) {
			float sine = Mathf.Sin( Time.time * bobSpeed );
			float offset = sine * bobStrength;
			offset += bobStrength;

			crystal.transform.parent.position = originalPos + offset * Vector3.up;
		}

		[Serializable]
		private struct EnemyConnections {
			public GameObject enemy;
			[NonSerialized] public GameObject lineDrawer;
			[NonSerialized] public CrystalLine lineComponent;
		}
	}
}
