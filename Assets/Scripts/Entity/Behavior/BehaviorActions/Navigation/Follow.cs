using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Navigation {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Follow",
		"Follows target until it's reached.",
		category: "WotT/Navigation",
		story: "[Agent] follows [Target]",
		id: "WotT_Follow"
	)]
	public class WotTFollow : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<GameObject> Target;

		[Tooltip( "The distance to the target that the agent calculates it's path to." )]
		[SerializeReference] public BlackboardVariable<float> FollowDistance = new( 0.5f );

		[Tooltip( "The distance to the target that the agent deems close enough to skip calculating a new path." )]
		[SerializeReference] public BlackboardVariable<float> DistanceTolerance = new( 1f );

		[Tooltip( "The max allowed distance from the navigation target to the actual target. When distance is above this number the path gets recalculated." )]
		[SerializeReference] public BlackboardVariable<float> AllowedTargetErrorDistance = new( 8f );

		[CreateProperty] private Vector3 currentTarget;

		private BehaviorAgentTranslator mAgentTranslatorCharacterBehaviorController;

		protected override Status OnStart( ) {
			if ( !Agent.Value ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			if ( !Target.Value ) {
				LogFailure( "No target to follow." );
				return Status.Failure;
			}

			if ( DistanceTolerance.Value <= FollowDistance.Value ) {
				LogFailure( "Distance Tolerance is lower or equal to Follow Distance, this will cause jittering when the agent reaches it's target and is not allowed." );
				return Status.Failure;
			}

			if ( AllowedTargetErrorDistance.Value < 1 ) {
				LogFailure( "Allowed Target Error Distance is lower than 1 and will cause the agent to recalculate it's path very often, please keep it above 1." );
				return Status.Failure;
			}

			if ( GetDistanceToCurrentTarget( ) <= DistanceTolerance.Value ) return Status.Success;

			Initialize( );
			UpdateFollowDestination( );

			return Status.Running;
		}

		protected override Status OnUpdate( ) {
			if ( !Agent.Value || !Target.Value ) return Status.Failure;

			if ( GetDistanceToCurrentTarget( ) <= DistanceTolerance.Value || mAgentTranslatorCharacterBehaviorController.movementState == BehaviorAgentTranslator.MovementState.Idle ) return Status.Success;

			float targetDistanceError = GetCurrentTargetDistanceToFollowedTarget( );
			if ( targetDistanceError >= AllowedTargetErrorDistance.Value ) UpdateFollowDestination( );

			return Status.Running;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { mAgentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }

		private void UpdateFollowDestination( ) { mAgentTranslatorCharacterBehaviorController.SetDestination( GetFollowTarget( ) ); }

		private float GetCurrentTargetDistanceToFollowedTarget( ) => ( currentTarget - Target.Value.transform.position ).magnitude;

		private float GetDistanceToCurrentTarget( ) => ( Target.Value.transform.position - Agent.Value.transform.position ).magnitude;

		private Vector3 GetFollowTarget( ) {
			currentTarget = ( Agent.Value.transform.position - Target.Value.transform.position ).normalized * FollowDistance.Value + Target.Value.transform.position;
			return currentTarget;
		}
	}
}
