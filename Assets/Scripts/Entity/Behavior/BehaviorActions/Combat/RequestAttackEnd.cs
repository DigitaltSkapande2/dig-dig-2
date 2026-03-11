using System;

using DigDig2.Combat;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Combat
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Request Attack End",
		"Request an attack's end.",
		category: "WotT/Combat",
		story: "[Agent] requests to end their attack",
		id: "WotT_Request_Attack_End"
	)]
	public class WotTCharge : Action
	{
		[SerializeReference] public BlackboardVariable<GameObject> Agent;

		private Attacker agentAttacker;

		protected override Status OnStart( )
		{
			if ( !Agent.Value )
			{
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );

			agentAttacker.RequestAttackEnd( );

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentAttacker = Agent.Value.GetComponentInChildren<Attacker>( ); }
	}
}
