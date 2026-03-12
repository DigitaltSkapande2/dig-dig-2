using System;
using System.Collections.Generic;

using DigDig2.Combat;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

namespace DigDig2.Entity.Behavior.BehaviorActions.Combat
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Scan For Enemy in Radius",
		"Scan for enemies around the agent.",
		category: "WotT/Combat",
		story: "[Agent] scans for an enemy in radius [Radius] and assigns it to [Variable]",
		id: "WotT_Scan_Enemy_Radius"
	)]
	public class WotTScanEnemyRadius : Action
	{
		public enum EnemyPriorityMode { Closest, Strongest, Random }

		private const int MAX_RANDOM_SCAN_TRIES = 10;

		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<float> Radius = new( 5f );
		[SerializeReference] public BlackboardVariable Variable;
		[SerializeReference] public BlackboardVariable<List<string>> AttackableFilter = new( );
		[SerializeReference] public BlackboardVariable<EnemyPriorityMode> EnemySelectionPriorityMode;

		private BehaviorAgentTranslator mAgentTranslatorCharacterBehaviorController;

		protected override Status OnStart( )
		{
			if ( Agent.Value == null )
			{
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			if ( AttackableFilter.Value.Count <= 0 )
			{
				LogFailure( "Attackable filter is empty." );
				return Status.Failure;
			}

			Initialize( );

			Collider[ ] colliders = Physics.OverlapSphere( mAgentTranslatorCharacterBehaviorController.transform.position, Radius );
			Attackable selectedEnemy = null;
			switch ( EnemySelectionPriorityMode.Value )
			{
				case EnemyPriorityMode.Closest:
					float closestEnemyDistance = -1f;
					foreach ( Collider enemyCollider in colliders )
					{
						Attackable enemyAttackable = enemyCollider.GetComponent<Attackable>( );
						if ( !enemyAttackable ) continue;
						if ( !AttackableFilter.Value.Contains( enemyAttackable.Group ) ) continue;

						if ( selectedEnemy && !( Vector3.Distance( enemyCollider.transform.position, mAgentTranslatorCharacterBehaviorController.transform.position ) < closestEnemyDistance ) ) continue;

						selectedEnemy = enemyAttackable;
						closestEnemyDistance = Vector3.Distance( selectedEnemy.transform.position, mAgentTranslatorCharacterBehaviorController.transform.position );
					}

					break;
				case EnemyPriorityMode.Strongest:
					LogFailure( "Strongest priority mode has not been implemented yet." );
					return Status.Failure;
				case EnemyPriorityMode.Random:
					int randomScanTries = 0;
					while ( true )
					{
						randomScanTries += 1;
						if ( randomScanTries >= MAX_RANDOM_SCAN_TRIES ) break;

						Attackable candidateAttackable = colliders[ Random.Range( 0, colliders.Length - 1 ) ].GetComponent<Attackable>( );
						if ( !candidateAttackable ) continue;
						if ( !AttackableFilter.Value.Contains( candidateAttackable.Group ) ) continue;

						selectedEnemy = candidateAttackable;
						break;
					}

					break;
				default: throw new ArgumentOutOfRangeException( );
			}

			Variable.ObjectValue = selectedEnemy;

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { mAgentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }
	}
}
