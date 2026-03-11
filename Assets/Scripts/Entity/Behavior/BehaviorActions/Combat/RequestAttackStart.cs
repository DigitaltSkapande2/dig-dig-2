using System;

using DigDig2.Combat;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Combat
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Request Attack Start",
		"Request an attack.",
		category: "WotT/Combat",
		story: "[Agent] requests to attack with attack number [AttackIndex]",
		id: "WotT_Request_Attack_Start"
	)]
	public class WotTRequestAttackStart : Action
	{
		[SerializeReference] public BlackboardVariable<GameObject> Agent;
		[SerializeReference] public BlackboardVariable<int> AttackIndex = new( 0 );

		private Attacker agentAttacker;

		protected override Status OnStart( )
		{
			if ( Agent.Value == null )
			{
				LogFailure( "No agent assigned." );
				return Status.Failure;
			}

			Initialize( );

			agentAttacker.RequestAttackStart( AttackIndex.Value );

			return Status.Success;
		}

		protected override void OnDeserialize( ) { Initialize( ); }

		private void Initialize( ) { agentAttacker = Agent.Value.GetComponentInChildren<Attacker>( ); }
	}
}
