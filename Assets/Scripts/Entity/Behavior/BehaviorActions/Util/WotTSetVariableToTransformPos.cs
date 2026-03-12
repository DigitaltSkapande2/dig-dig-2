using System;

using Unity.Behavior;
using Unity.Properties;

using UnityEngine;

using Action = Unity.Behavior.Action;

namespace DigDig2.Entity.Behavior.BehaviorActions.Util
{
	[Serializable] [GeneratePropertyBag] [NodeDescription(
		"WotT Set Vector3 to Transform Position",
		"Sets a vector3 variable to a GameObject's position.",
		category: "WotT/Util",
		story: "Set [Variable] to [Transform] position",
		id: "WotT_Set_Vector3_To_Transform_Position"
	)]
	public class WotTSetVector3ToTransformPosition : Action
	{
		[SerializeReference] public BlackboardVariable Variable;
		[SerializeReference] public BlackboardVariable<Transform> Transform;

		protected override Status OnStart( )
		{
			if ( Variable == null )
			{
				LogFailure( "No variable assigned." );
				return Status.Failure;
			}

			if ( !Transform.Value )
			{
				LogFailure( "No transform assigned." );
				return Status.Failure;
			}

			Variable.ObjectValue = Transform.Value.position;

			return Status.Success;
		}
	}
}
