using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Visuals
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Set Auto Look Lock",
		"Toggle the EntityCharacterController movement look following, where the entity looks in the direction it is moving.",
		category: "WotT/Visuals",
		story: "[Agent] sets auto look lock to [State]",
		id: "WotT_Auto_Look_Lock"
	)]
	public class WotTAutoLookLock : Action
	{
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<bool> State = new( false );

		private BehaviorAgentTranslator mAgentTranslatorCharacterBehaviorController;

		protected override Status OnStart( )
		{
			if ( Agent.Value == null )
			{
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );
			mAgentTranslatorCharacterBehaviorController.SetAutomaticLookRotationLock( State.Value );

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { mAgentTranslatorCharacterBehaviorController = Agent.Value.GetComponentInChildren<BehaviorAgentTranslator>( ); }
	}
}
