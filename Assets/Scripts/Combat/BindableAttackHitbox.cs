using System;
using System.Collections.Generic;

using DigDig2.Combat.Attacks;

using UnityEngine;

namespace DigDig2.Combat
{
	public class BindableAttackHitbox : MonoBehaviour
	{
		[SerializeField] private AttackHitboxShape shape = AttackHitboxShape.Box;
		[SerializeField] private Vector3 boxSize = Vector3.one;
		[SerializeField] private float sphereRadius = 1.0f;
		[SerializeField] private float unitsPerIntermediateCheck = 0.001f;

		private readonly Dictionary<string, AttackInfo> activeAttacks = new( );

		private void OnDrawGizmos( )
		{
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = Color.red;
			if ( activeAttacks.Count > 0 ) Gizmos.color = Color.blue;
			switch ( shape )
			{
				case AttackHitboxShape.Box: Gizmos.DrawWireCube( Vector3.zero, boxSize ); break;
				case AttackHitboxShape.Sphere: Gizmos.DrawSphere( Vector3.zero, sphereRadius ); break;
				default: throw new ArgumentOutOfRangeException( );
			}
		}

		public void StartAttack( string attackId, Attacker attacker, Attack attack )
		{
			Attackable attackerAttackable = attacker.GetComponent<Attackable>( );

			activeAttacks[ attackId ] = new( )
			{
				attacker = attacker,
				attackerAttackable = attackerAttackable,
				attack = attack,
				attackedEntities = new( ),
				hasCheckedOnce = false
			};
		}

		public void Attack( string attackId )
		{
			AttackInfo attackInfo = activeAttacks[ attackId ];

			int intermediateAttacks = 0;
			if ( attackInfo.hasCheckedOnce )
			{
				float distanceBetweenChecks = Vector3.Distance( attackInfo.lastPosition, transform.position );
				intermediateAttacks = Mathf.CeilToInt( distanceBetweenChecks / unitsPerIntermediateCheck );
			}

			for ( int intermediateAttackIndex = 0;
				intermediateAttackIndex < intermediateAttacks;
				intermediateAttackIndex++ )
			{
				var intermediatePosition = Vector3.Lerp( attackInfo.lastPosition, transform.position, (float)intermediateAttackIndex / intermediateAttacks );
				var intermediateRotation = Quaternion.Slerp( attackInfo.lastRotation, transform.rotation, (float)intermediateAttackIndex / intermediateAttacks );
				Debug.DrawLine( intermediatePosition, intermediatePosition + Vector3.up / 10f, Color.blue, 1f );
				Collider[ ] enemyColliders = shape switch
				{
					AttackHitboxShape.Box => Physics.OverlapBox(
						intermediatePosition,
						new(
							boxSize.x / 2 * transform.lossyScale.x,
							boxSize.y / 2 * transform.lossyScale.y,
							boxSize.z / 2 * transform.lossyScale.z
						),
						intermediateRotation
					),
					AttackHitboxShape.Sphere => Physics.OverlapSphere( intermediatePosition, sphereRadius ),
					_ => Array.Empty<Collider>( )
				};

				foreach ( Collider enemyCollider in enemyColliders )
				{
					Attackable enemyAttackable = enemyCollider.GetComponent<Attackable>( );
					if ( !enemyAttackable ) continue;
					if ( enemyAttackable == attackInfo.attackerAttackable ) continue;
					if ( attackInfo.attackedEntities.Contains( enemyAttackable ) ) continue;
					if ( enemyAttackable.Group.Contains( "Melee Only" ) && attackInfo.attack is RangedAttack ) continue;

					attackInfo.attackedEntities.Add( enemyAttackable );
					enemyAttackable.Hit( attackInfo.attack, attackInfo.attacker );
				}
			}

			attackInfo.lastPosition = transform.position;
			attackInfo.lastRotation = transform.rotation;
			attackInfo.hasCheckedOnce = true;
			activeAttacks[ attackId ] = attackInfo;
		}

		public void EndAttack( string attackId ) { activeAttacks.Remove( attackId ); }
		private enum AttackHitboxShape { Box, Sphere }

		private struct AttackInfo
		{
			public Attacker attacker;
			public Attackable attackerAttackable;
			public Attack attack;
			public List<Attackable> attackedEntities;
			public bool hasCheckedOnce;
			public Vector3 lastPosition;
			public Quaternion lastRotation;
		}
	}
}
