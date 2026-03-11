using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Visuals
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Focus On",
		"Tell the agent to continuously look towards a transform.",
		category: "WotT/Visuals",
		story: "[Agent] focuses on [Transform]",
		id: "WotT_Focus_On"
	)]
	public class WotTFocusOn : Action
	{
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<Transform> Transform;

		private BehaviorAgentTranslator agentTranslatorCharacterBehaviorController;

		protected override Status OnStart( )
		{
			if ( !Agent.Value )
			{
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			if ( !Transform.Value )
			{
				LogFailure( "No transform assigned." );
				return Status.Failure;
			}

			Initialize( );
			agentTranslatorCharacterBehaviorController.SetFocusedLookTransform( Transform.Value );

			return Status.Success;
		}

		protected override Status OnUpdate( )
		{
			agentTranslatorCharacterBehaviorController.LookTowards( Transform.Value.position );
			return Status.Running;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }
	}
}
