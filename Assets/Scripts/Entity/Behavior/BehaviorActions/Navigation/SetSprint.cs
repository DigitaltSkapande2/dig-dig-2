using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Navigation {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Set Sprint Mode",
		"Toggle if the entity is sprinting or not.",
		category: "WotT/Navigation",
		story: "[Agent] sets sprint mode to [State]",
		id: "WotT_Set_Sprint"
	)]
	public class WotTSetSprint : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<bool> State = new( false );

		private BehaviorAgentTranslator agentTranslatorCharacterBehaviorController;

		protected override Status OnStart( ) {
			if ( Agent.Value == null ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );

			// Commented out because we might not need sprinting
			// agentCharacterBehaviorInputController.SetSprintMode(State.Value);

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }
	}
}
