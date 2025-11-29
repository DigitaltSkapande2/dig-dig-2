using UnityEngine;
using System;
using Unity.Behavior;
using Unity.Properties;

namespace DigDig2
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "WotT Set Vector3 to Transform Position",
        description: "Sets a vector3 variable to a GameObject's position.",
        category: "WotT/General Utils",
        story: "Set [Variable] to [Transform] position",
        id: "WotT_Set_Vector3_To_Transform_Position"
    )]
    public partial class WotTSetVector3ToTransformPosition : Unity.Behavior.Action
    {
        [SerializeReference] public BlackboardVariable Variable;
        [SerializeReference] public BlackboardVariable<Transform> Transform;



        protected override Status OnStart()
        {
            if (Variable == null)
            {
                LogFailure("No variable assigned.");
                return Status.Failure;
            }

            if (Transform.Value == null)
            {
                LogFailure("No transform assigned.");
                return Status.Failure;
            }

			Variable.ObjectValue = Transform.Value.position;

            return Status.Success;
        }
    }
}
