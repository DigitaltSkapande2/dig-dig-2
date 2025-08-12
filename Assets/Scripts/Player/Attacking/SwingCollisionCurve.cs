using UnityEngine;

namespace DigDig2
{
    public class SwingCollisionCurve : MonoBehaviour
    {
        [SerializeField] GameObject hitbox;
        [SerializeField] Curve3D curve;
        [SerializeField] float step;

        void Start()
        {
            Vector3 pos = new Vector3(curve.X.Evaluate(0), curve.Y.Evaluate(0), curve.Z.Evaluate(0));
            Vector3 stepPos = new Vector3(curve.X.Evaluate(step), curve.Y.Evaluate(step), curve.Z.Evaluate(step));

            Vector3 dir = (stepPos - pos).normalized;
        }
    }
}
