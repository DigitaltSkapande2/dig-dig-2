using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DigDig2.Combat;
using DigDig2.SaveSystem;
using Unity.Mathematics;

using UnityEngine;

namespace DigDig2.Game
{
	[RequireComponent( typeof( Attackable ), typeof( Health ) )]
	public class Crystal : MonoBehaviour, IOrderedPropSaveable
	{
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
        [SerializeField] private string shieldDissolveAmountFloatName;
        [SerializeField] private string shieldVoronoiColorName;

        [SerializeField, ColorUsage(true, true)]
        private Color deactivatedShieldTargetVoronoiColor;

        [SerializeField] private float deactivatedShieldTargetScale;
        [SerializeField] private AnimationCurve shieldDissolveCurve;
        [SerializeField] private float shieldDissolveAnimationDuration;

		[Tooltip( "The line prefab." )]
		[SerializeField] private GameObject linePrefab;

		[Tooltip( "How far the crystal bobs up and down" )]
		[SerializeField] private float bobStrength;

		[Tooltip( "How fast the crystal bobs up and down" )]
		[SerializeField] private float bobSpeed;

        [Header("Ocean")]
        [SerializeField] private OceanFollow ocean;
        [SerializeField] private float oceanLowerAmount;

		private Attackable attackable;
		private MeshRenderer crystalMeshRenderer;
        private MeshRenderer shieldMeshRenderer;

		private bool hasShield = true;
		private Health health;

		private Vector3 originalPos;

		private void Awake( )
		{
			attackable = GetComponent<Attackable>( );
			health = GetComponent<Health>( );
            
            health.death.AddListener(OnDeath);

			attackable.hit.AddListener( OnHit );
		}

		private void Start( )
		{
			if ( crystal == null )
			{
				Debug.LogError( "Crystal has not been assigned." );
				return;
			}

			crystalMeshRenderer = crystal.GetComponent<MeshRenderer>( );
            shieldMeshRenderer = shield.GetComponent<MeshRenderer>( );

			Material material = crystalMeshRenderer.material;
			material.SetFloat( crackStage, NO_CRACK_STAGE );
            
            if ( enemyConnections.Count <= 0 ) DeactivateShield(true);

			originalPos = transform.position;
		}

		private void Update( )
		{
			HeightBob( );

			for ( int index = enemyConnections.Count - 1; index >= 0; index-- )
			{
				EnemyConnections enemyConnection = enemyConnections[ index ];
				if ( !enemyConnection.lineDrawer && !enemyConnection.enemy ) continue;

				if ( !enemyConnection.lineDrawer )
				{
					enemyConnection.lineDrawer = Instantiate( linePrefab, transform.position, quaternion.identity, transform );
					enemyConnection.lineComponent = enemyConnection.lineDrawer.GetComponent<CrystalLine>( );
					enemyConnections[ index ] = enemyConnection;
				}

				if ( !enemyConnection.enemy )
				{
					Destroy( enemyConnection.lineDrawer );
					enemyConnections.RemoveAt( index );

					continue;
				}

				enemyConnection.lineComponent.SetPositions( transform.position, enemyConnection.enemy.transform.position );
			}

			if ( enemyConnections.Count <= 0 ) DeactivateShield(false);
		}

        private void OnDeath(GameObject _)
        {
            ocean.LowerWater(oceanLowerAmount);
        }

		private void OnHit( )
		{
			if ( hasShield ) return;

			Material material = crystalMeshRenderer.material;

			switch ( health.HealthPoints )
			{
				case 2: material.SetFloat( crackStage, LIGHT_CRACK_STAGE ); break;
				case 1: material.SetFloat( crackStage, HEAVY_CRACK_STAGE ); break;
			}
		}

		private void HeightBob( )
		{
			float sine = Mathf.Sin( Time.time * bobSpeed );
			float offset = sine * bobStrength;
			offset += bobStrength;

			crystal.transform.parent.position = originalPos + offset * Vector3.up;
		}

        private void DeactivateShield(bool instant)
        {
            if (!hasShield) return;
            hasShield = false;
            health.enabled = true;
            attackable.enabled = true;
            if (instant)
            {
                shield.SetActive(false);
                return;
            }
            
            DissolveShield().Forget();
        }

        private async UniTask DissolveShield()
        {
            float startTime = Time.time;
            float timeElapsed = 0f;
            
            // Get default
            Color defaultVoronoiColor = shieldMeshRenderer.material.GetColor(shieldVoronoiColorName);
            Vector3 startScale = shield.transform.localScale;

            while (timeElapsed < shieldDissolveAnimationDuration)
            {
                await UniTask.Yield(PlayerLoopTiming.Update);

                timeElapsed = (Time.time - startTime);
                float t = timeElapsed / shieldDissolveAnimationDuration;
                float curveVal = shieldDissolveCurve.Evaluate(t);
            
                shieldMeshRenderer.material.SetFloat(shieldDissolveAmountFloatName, curveVal);
                shieldMeshRenderer.material.SetColor(shieldVoronoiColorName, Color.Lerp(defaultVoronoiColor, deactivatedShieldTargetVoronoiColor, curveVal));
                shield.transform.localScale = Vector3.Lerp(startScale, Vector3.one * deactivatedShieldTargetScale, curveVal);
            }
            
            shieldMeshRenderer.material.SetFloat(shieldDissolveAmountFloatName, 1f);
            shieldMeshRenderer.material.SetColor(shieldVoronoiColorName, Color.Lerp(defaultVoronoiColor, deactivatedShieldTargetVoronoiColor, 1f));
            shield.SetActive(false);
        }

		[Serializable]
		private struct EnemyConnections
		{
			public GameObject enemy;
			[NonSerialized] public GameObject lineDrawer;
			[NonSerialized] public CrystalLine lineComponent;
		}

        public void OnLoaded(int myId, int activeId, OrderedPropSaver propSaver)
        {
            if (myId < activeId)
            {
                ocean.LowerWater(oceanLowerAmount, true);
                Destroy(gameObject);
                return;
            }

            if (myId == activeId)
            {
                
            }
            
            health.death.AddListener((_) => propSaver.IncrementActiveIndex()); 
        }
    }
}
