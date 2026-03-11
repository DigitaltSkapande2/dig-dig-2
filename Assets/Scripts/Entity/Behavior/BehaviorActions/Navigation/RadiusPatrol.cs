using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

namespace DigDig2.Entity.Behavior.BehaviorActions.Navigation {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Radius Patrol",
		"Randomly patrols the agent around a point with a certain radius.",
		category: "WotT/Navigation",
		story: "[Agent] patrols around [Center] with a max radius of [MaxRadius]",
		id: "WotT_Radius_Patrol"
	)]
	public class WotTRadiusPatrol : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;

		[Tooltip( "The middle point of the patrol circle/radius." )]
		[SerializeReference] public BlackboardVariable<Vector3> Center;

		[Tooltip( "The max distance from the middle point that the agent will patrol to." )]
		[SerializeReference] public BlackboardVariable<float> MaxRadius = new( 5f );

		[Tooltip( "The min distance from the middle point that the agent will patrol to." )]
		[SerializeReference] public BlackboardVariable<float> MinRadius = new( 0.1f );

		[Tooltip( "How long the agent will wait after reaching a waypoint to find a new waypoint." )]
		[SerializeReference] public BlackboardVariable<float> WaypointWaitTime = new( 1.0f );

		[Tooltip( "The minimum distance that the new waypoint has to be from the last waypoint to be considered a valid patrol waypoint." )]
		[SerializeReference] public BlackboardVariable<float> AllowedDistanceBetweenLastAndNewPoint = new( 1.5f );

		[Tooltip( "The maximum amount of tries the agent will attempt to select a new waypoint." )]
		[SerializeReference] public BlackboardVariable<int> MaxPointSelectionAttempts = new( 5 );

		private BehaviorAgentTranslator agentTranslatorCharacterBehaviorController;

		[CreateProperty] private Vector3 lastPoint;
		[CreateProperty] private bool waiting;
		[CreateProperty] private float waypointWaitTimer;

		protected override Status OnStart( ) {
			if ( !Agent.Value ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );

			waiting = false;
			waypointWaitTimer = 0.0f;

			MoveToNextRandomPoint( );
			return Status.Running;
		}

		protected override Status OnUpdate( ) {
			if ( !Agent.Value ) return Status.Failure;

			if ( waiting ) {
				if ( waypointWaitTimer > 0.0f )
					waypointWaitTimer -= Time.deltaTime;
				else {
					waypointWaitTimer = 0f;
					waiting = false;
					MoveToNextRandomPoint( );
				}
			} else {
				// Check if we've reached the waypoint (ensuring NavMeshAgent has completed path calculation if available)
				if ( agentTranslatorCharacterBehaviorController && agentTranslatorCharacterBehaviorController.movementState != BehaviorAgentTranslator.MovementState.Idle ) return Status.Running;

				waypointWaitTimer = WaypointWaitTime.Value;
				waiting = true;
			}

			return Status.Running;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }

		private void MoveToNextRandomPoint( ) {
			int targetPointSelectionAttempts = 0;
			Vector3 targetPoint;
			while ( true ) {
				float randomRotation = Random.Range( 0, 360 );
				float randomRadius = Random.Range( MinRadius.Value, MaxRadius.Value );
				Vector3 directionNormal = new( Mathf.Cos( randomRotation * Mathf.Deg2Rad ), 0f, Mathf.Sin( randomRotation * Mathf.Deg2Rad ) );

				targetPoint = Center.Value + directionNormal * randomRadius;

				if ( Vector3.Distance( targetPoint, lastPoint ) >= AllowedDistanceBetweenLastAndNewPoint.Value ) break;

				targetPointSelectionAttempts++;
				if ( targetPointSelectionAttempts < MaxPointSelectionAttempts.Value ) continue;

				Debug.LogWarning( "Agent radius patrol reached maximum point selection attempts, please consider lowering the Allowed Distance Between Last And New Point or increase the Max Point Selection Attempts." );
				break;
			}

			agentTranslatorCharacterBehaviorController.SetDestination( targetPoint );

			lastPoint = targetPoint;
		}
	}
}
