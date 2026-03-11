using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Visuals {
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Look Towards",
		"Tell the agent to look towards a point.",
		category: "WotT/Visuals",
		story: "[Agent] looks towards [Point]",
		id: "WotT_Look_Towards"
	)]
	public class WotTLookTowards : Action {
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<Vector3> Point;

		private BehaviorAgentTranslator agentTranslatorCharacterBehaviorController;

		protected override Status OnStart( ) {
			if ( !Agent.Value ) {
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );
			agentTranslatorCharacterBehaviorController.LookTowards( Point.Value );

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }
	}
}
