using DigDig2.Combat;

using UnityEngine;

namespace DigDig2.Debugging.Combat {
	[RequireComponent( typeof( MeshRenderer ), typeof( Attackable ) )]
	public class AttackIndicator : MonoBehaviour {
		private Attackable attackable;
		private MeshRenderer meshRenderer;

		private void Awake( ) {
			meshRenderer = GetComponent<MeshRenderer>( );

			attackable = GetComponent<Attackable>( );
			attackable.hit.AddListener( OnAttack );
		}

		private void OnAttack( ) { meshRenderer.material.color = new( Random.Range( 0f, 1f ), Random.Range( 0f, 1f ), Random.Range( 0f, 1f ) ); }
	}
}
