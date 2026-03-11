using System;
using System.Collections.Generic;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;
using Random = UnityEngine.Random;

namespace DigDig2.Entity.Behavior.BehaviorActions.Navigation {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Waypoint Patrol",
		"Patrols the agent along a set of waypoints.",
		category: "WotT/Navigation",
		story: "[Agent] patrols along [Waypoints]",
		id: "WotT_Waypoint_Patrol"
	)]
	public class WotTWaypointPatrol : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;

		[Tooltip( "The waypoints to patrol around." )] [SerializeReference]
		public BlackboardVariable<List<GameObject>> Waypoints;

		[Tooltip( "How long the agent will wait after reaching a waypoint to find a new waypoint." )] [SerializeReference]
		public BlackboardVariable<float> WaypointWaitTime = new( 1.0f );

		[Tooltip( "If the agent should select a waypoint in the list at random, or in order. This will not select the same waypoint that the agent just patrolled to." )] [SerializeReference]
		public BlackboardVariable<bool> RandomizeWaypointSelection = new( false );

		[CreateProperty] private int currentPatrolPoint;
		[CreateProperty] private Vector3 currentTarget;

		private BehaviorAgentTranslator mAgentTranslatorCharacterBehaviorController;
		[CreateProperty] private bool waiting;
		[CreateProperty] private float waypointWaitTimer;

		protected override Status OnStart( ) {
			if ( Agent.Value == null ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			if ( Waypoints.Value == null || Waypoints.Value.Count == 0 ) {
				LogFailure( "No waypoints to patrol assigned." );
				return Status.Failure;
			}

			Initialize( );

			waiting = false;
			waypointWaitTimer = 0.0f;

			MoveToNextWaypoint( );
			return Status.Running;
		}

		protected override Status OnUpdate( ) {
			if ( Agent.Value == null || Waypoints.Value == null ) return Status.Failure;

			if ( waiting ) {
				if ( waypointWaitTimer > 0.0f )
					waypointWaitTimer -= Time.deltaTime;
				else {
					waypointWaitTimer = 0f;
					waiting = false;
					MoveToNextWaypoint( );
				}
			} else {
				// Check if we've reached the waypoint (ensuring NavMeshAgent has completed path calculation if available)
				if ( mAgentTranslatorCharacterBehaviorController == null || mAgentTranslatorCharacterBehaviorController.movementState == BehaviorAgentTranslator.MovementState.Idle ) {
					waypointWaitTimer = WaypointWaitTime.Value;
					waiting = true;

					return Status.Running;
				}
			}

			return Status.Running;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { mAgentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }

		private void MoveToNextWaypoint( ) {
			if ( !RandomizeWaypointSelection.Value )
				currentPatrolPoint = ( currentPatrolPoint + 1 ) % Waypoints.Value.Count;
			else {
				int lastPatrolPoint = currentPatrolPoint;
				while ( lastPatrolPoint == currentPatrolPoint ) { currentPatrolPoint = Random.Range( 0, Waypoints.Value.Count - 1 ); }
			}

			currentTarget = Waypoints.Value[ currentPatrolPoint ].transform.position;
			if ( mAgentTranslatorCharacterBehaviorController != null ) mAgentTranslatorCharacterBehaviorController.SetDestination( currentTarget );
		}
	}
}
