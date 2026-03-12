using DigDig2.EffectSystem;

using UnityEditor;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace DigDig2.Combat
{
	[RequireComponent( typeof( Attackable ) )]
	public class Health : MonoBehaviour
	{
		[Tooltip( "Starting health and eventual cap for healing." )]
		[SerializeField] private int maxHealthPoints = 1;

		[Tooltip( "The entity's current health." )]
		[SerializeField] private int healthPoints = 1;

		[FormerlySerializedAs( "DestroyOnDeath" )] [SerializeField]
		private bool destroyOnDeath = true;

		[Tooltip( "Effects to be played when health is below 0." )]
		[SerializeField] private EffectPlayer deathEffectPlayer;

		[Tooltip( "Event is called when health is below 0." )]
		[SerializeField] public UnityEvent death;

		[SerializeField] public UnityEvent<int> healthChanged;

		public int MaxHealthPoints
		{
			get => maxHealthPoints;
		}

		public int HealthPoints
		{
			get => healthPoints;
			set => SetHealth( value );
		}

		private void Start( ) { SetHealth( healthPoints ); }

		public void Damage( int damage )
		{
			if ( !enabled ) return;

			SetHealth( healthPoints - damage );
		}

		public void Heal( int amount )
		{
			if ( !enabled ) return;

			SetHealth( healthPoints + amount );
		}

		public void SetHealth( int newHealth )
		{
			healthPoints = Mathf.Clamp( newHealth, 0, maxHealthPoints );
			healthChanged.Invoke( healthPoints );
			CheckState( );
		}

		public void Kill( )
		{
			healthPoints = 0;

			death.Invoke( );
			deathEffectPlayer.Play( transform.position, Quaternion.identity, Vector3.one, transform.parent );
			if ( destroyOnDeath )
				Destroy( gameObject );
			else
				enabled = false;
		}

		private void CheckState( )
		{
			if ( healthPoints <= 0 ) Kill( );
		}
	}

	#if UNITY_EDITOR

	[CustomEditor( typeof( Health ) )]
	public class HealthEditor : Editor
	{
		public override void OnInspectorGUI( )
		{
			base.OnInspectorGUI( );

			var health = (Health)target;

			if ( GUILayout.Button( "Damage 1" ) ) health.Damage( 1 );
		}
	}

	#endif
}
