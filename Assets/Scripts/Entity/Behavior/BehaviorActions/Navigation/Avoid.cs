using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Navigation {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Avoid",
		"Runs away from the target for a certain amount of seconds.",
		category: "WotT/Navigation",
		story: "[Agent] avoids [Target] for [Duration] seconds",
		id: "WotT_Avoid"
	)]
	public class WotTAvoid : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;

		[Tooltip( "The target to avoid." )]
		[SerializeReference] public BlackboardVariable<GameObject> Target;

		[Tooltip( "How long the agent will avoid the target for." )]
		[SerializeReference] public BlackboardVariable<float> Duration;

		[CreateProperty] private float avoidTimer;

		private BehaviorAgentTranslator mAgentTranslatorCharacterBehaviorController;

		protected override Status OnStart( ) {
			if ( Agent.Value == null ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			if ( Target.Value == null ) {
				LogFailure( "No target object assigned." );
				return Status.Failure;
			}

			avoidTimer = Duration.Value;

			Initialize( );
			UpdateAvoidDirection( );

			return Status.Running;
		}

		protected override Status OnUpdate( ) {
			if ( !Agent.Value || !Target.Value ) return Status.Failure;

			avoidTimer -= Time.deltaTime;
			if ( avoidTimer <= 0 ) {
				avoidTimer = 0;
				mAgentTranslatorCharacterBehaviorController.Stop( );
				return Status.Success;
			}

			UpdateAvoidDirection( );

			return Status.Running;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { mAgentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }

		private void UpdateAvoidDirection( ) { mAgentTranslatorCharacterBehaviorController.SetDirection( GetAvoidDirection( ) ); }

		private Vector3 GetAvoidDirection( ) => -( Target.Value.transform.position - Agent.Value.transform.position ).normalized;
	}
}
