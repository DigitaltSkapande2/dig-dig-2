using System;

using UnityEngine;
using UnityEngine.AI;

namespace DigDig2.Entity.Behavior
{
	[RequireComponent( typeof( EntityCharacterController ) )]
	public class BehaviorAgentTranslator : MonoBehaviour
	{
		public enum MovementState { Idle, FollowingPath, FollowingDirection }

		[Header( "Following Path State" )]
		[Tooltip( "The distance that the entity marks a waypoint as passed." )]
		[SerializeField] private float pathWaypointDistanceTolerance = 1f;

		private EntityCharacterController entityCharacterController;

		#region Visuals State Variables

		private Transform focusedTransform;

		#endregion

		private void Awake( ) { entityCharacterController = GetComponent<EntityCharacterController>( ); }

		private void Start( ) { navMeshPath = new( ); }

		private void Update( )
		{
			if ( focusedTransform ) LookTowards( focusedTransform.position );

			switch ( movementState )
			{
				case MovementState.FollowingPath:
					if ( currentPathWaypoints.Length > 0 )
					{
						Vector3 currentPathWaypoint = currentPathWaypoints[ currentPathWaypointIndex ];
						Vector3 positionDifference = currentPathWaypoint - transform.position;
						positionDifference.y = 0f;
						float distanceToWaypoint = positionDifference.magnitude;

						if ( distanceToWaypoint <= pathWaypointDistanceTolerance )
						{
							if ( currentPathWaypoints.Length > currentPathWaypointIndex + 1 )
							{
								// More waypoints to follow, go to next one
								currentPathWaypointIndex++;
								currentPathWaypoint = currentPathWaypoints[ currentPathWaypointIndex ];
								positionDifference = currentPathWaypoint - transform.position;
								positionDifference.y = 0f;
								distanceToWaypoint = positionDifference.magnitude;
							}
							else
							{
								// No more waypoints to follow, entity has finished, reset path
								Stop( );
							}
						}

						if ( currentPathWaypointIndex >= currentPathWaypoints.Length - 1 )
							entityCharacterController.inputMoveVector = positionDifference.normalized * Mathf.Min( distanceToWaypoint / ( pathWaypointDistanceTolerance + 1f ), 1f );
						else
							entityCharacterController.inputMoveVector = positionDifference.normalized;
					}

					break;
				case MovementState.FollowingDirection: entityCharacterController.inputMoveVector = currentDirection; break;
				default: entityCharacterController.inputMoveVector = Vector3.zero; break;
			}
		}

		private void OnDrawGizmosSelected( )
		{
			if ( !entityCharacterController ) return;

			switch ( movementState )
			{
				case MovementState.FollowingPath:
					for ( int pathWaypointIndex = currentPathWaypointIndex + 1; pathWaypointIndex < currentPathWaypoints.Length; pathWaypointIndex++ )
					{
						Gizmos.color = Color.gray;
						Gizmos.DrawSphere( currentPathWaypoints[ pathWaypointIndex ], 0.25f );
					}

					Gizmos.color = Color.red;
					if ( currentPathWaypoints.Length - 1 >= currentPathWaypointIndex ) Gizmos.DrawSphere( currentPathWaypoints[ currentPathWaypointIndex ], 0.5f );
					break;
			}

			Gizmos.DrawLine( transform.position, transform.position + entityCharacterController.inputMoveVector );
		}

		#region Movement State Variables

		[SerializeField] public MovementState movementState = MovementState.Idle;

		// Following Path
		[NonSerialized] public Vector3 currentDestination = Vector3.zero;

		[NonSerialized] public Vector3[ ] currentPathWaypoints =
			{ };

		[NonSerialized] public int currentPathWaypointIndex;
		private NavMeshPath navMeshPath;

		// Following Direction
		[NonSerialized] public Vector3 currentDirection = Vector3.zero;

		#endregion

		#region State Management

		public void Stop( )
		{
			movementState = MovementState.Idle;

			// Reset Following Path State
			currentDestination = Vector3.zero;
			Array.Clear( currentPathWaypoints, 0, currentPathWaypoints.Length );
			currentPathWaypointIndex = 0;
			navMeshPath.ClearCorners( );

			// Reset Follow Direction State
			currentDirection = Vector3.zero;
		}

		public bool SetDestination( Vector3 destination )
		{
			Stop( );

			if ( navMeshPath == null ) return false;

			bool validPathFound = NavMesh.CalculatePath( transform.position, destination, 1, navMeshPath );
			if ( !validPathFound )
			{
				Debug.LogWarning( "Path could not be calculated." );
				return false;
			}

			currentDestination = destination;
			currentPathWaypoints = navMeshPath.corners;

			movementState = MovementState.FollowingPath;

			return true;
		}

		public bool SetDirection( Vector3 direction )
		{
			Stop( );

			currentDirection = direction;

			movementState = MovementState.FollowingDirection;

			return true;
		}

		public bool LookTowards( Vector3 target )
		{
			entityCharacterController.LookTowards( target );

			return true;
		}

		public bool SetFocusedLookTransform( Transform newFocusedTransform )
		{
			focusedTransform = newFocusedTransform;

			return true;
		}

		public Transform GetFocusedLookTransform( ) => focusedTransform;

		public bool SetAutomaticLookRotationLock( bool isLocked )
		{
			entityCharacterController.SetAutomaticLookRotationLock( isLocked );

			return true;
		}

		#endregion
	}
}
