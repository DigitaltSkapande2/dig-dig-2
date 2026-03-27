using System.Collections.Generic;

using DigDig2.CinemaCamera;
using DigDig2.EffectSystem;
using DigDig2.SaveSystem;

using Newtonsoft.Json;

using UnityEngine;

namespace DigDig2.Game
{
	public class OceanFollow : MonoBehaviour, ISaveable
	{
		[SerializeField] private Transform plane;

		[Tooltip( "the interval at witch to snap to. The sice of one grid piece" )]
		[SerializeField] private Vector2 gridSize = new( 1f, 1f );

		[SerializeField] private EffectPlayer onWaterLowerEffect;
		[SerializeField] private float verticalSpeed;

		[SerializeField] private List<ParticleSystem> waterSplashParticles = new( );

		private float targetY;
		private bool waterParticlesPlaying;

        private void Start( )
		{
			targetY = transform.position.y;

			SaveManager.Instance.RegisterSavable( "Ocean", this );
		}

		private void Update( )
		{
			transform.position = new( 0, Mathf.Lerp( transform.position.y, targetY, Time.deltaTime * verticalSpeed ), 0 );
            Plane waterPlane = new(Vector3.up, new Vector3(0f, transform.position.y, 0f));

            waterPlane.Raycast(new Ray(GameCamera.Instance.mainCamera.transform.position, GameCamera.Instance.mainCamera.transform.forward), out float raycastDistance);
            Vector3 cameraViewPositionOnPlane = GameCamera.Instance.mainCamera.transform.position + GameCamera.Instance.mainCamera.transform.forward * raycastDistance;

			var newPosition = new Vector3(
				Mathf.Round( cameraViewPositionOnPlane.x / gridSize.x ) * gridSize.x,
				transform.position.y,
				Mathf.Round( cameraViewPositionOnPlane.z / gridSize.y ) * gridSize.y
			);

			if ( waterParticlesPlaying && Mathf.Abs( Mathf.Abs( transform.position.y ) - Mathf.Abs( targetY ) ) < 1 )
			{
				foreach ( ParticleSystem ps in waterSplashParticles )
				{
					ps.Stop( );
					Debug.Log( "STOPPING water splash particles." );
				}

				waterParticlesPlaying = false;
			}

            plane.position = newPosition;
        }

		public object CollectData( ) => targetY;

		public void RestoreState( object dataObject )
		{
			if ( dataObject == null )
				Debug.Log( "WÄÄÄÄÄÄÄÄ" );
			else
			{
				try { targetY = (float)dataObject; }
				catch { targetY = JsonConvert.DeserializeObject<float>( dataObject.ToString( ) ); }

				print( targetY );
				transform.position = new( 0, targetY, 0 );
			}
		}

		public void LowerWater( float amount )
		{
			targetY -= amount;
			onWaterLowerEffect?.Play( );
			Debug.Log( "Lowering water. New target Y: " + targetY );
			foreach ( ParticleSystem ps in waterSplashParticles )
			{
				ps.Play( );
				Debug.Log( "Playing water splash particles." );
			}

			waterParticlesPlaying = true;
		}
	}
}
