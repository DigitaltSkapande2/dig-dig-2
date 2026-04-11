using DigDig2.EffectSystem;
using DigDig2.Entity;
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

		[SerializeField] private int damageThreshold = 1;

		[FormerlySerializedAs( "DestroyOnDeath" )] [SerializeField]
		private bool destroyOnDeath = true;

		[SerializeField] float deathWaitTime;

		[Tooltip( "Effects to be played when health is below 0." )]
		[SerializeField] private EffectPlayer deathEffectPlayer;

		[Tooltip( "Event is called when health is below 0." )]
		[SerializeField] public UnityEvent<GameObject> death;

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

        public bool IsAlive => healthPoints > 0;

		private void Start( ) { SetHealth( healthPoints ); }

		public void Damage( int damage )
		{
			if ( !enabled ) return;
			if ( damage < damageThreshold) return;

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

			death.Invoke(gameObject);
			deathEffectPlayer?.Play( transform.position, Quaternion.identity, Vector3.one, transform.parent );
			if ( destroyOnDeath )
			{
				if (TryGetComponent(out EntityCharacterController entityCharacterController)) entityCharacterController.enabled = false;
				if (TryGetComponent(out Animator animator)) animator.enabled = false;
				GetComponent<Attackable>().enabled = false;
				Destroy( gameObject, deathWaitTime );
			}

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
