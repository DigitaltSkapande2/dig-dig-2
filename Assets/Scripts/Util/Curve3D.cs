using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "Curve3D", menuName = "ScriptableObjects/Curve3D")]
    public class Curve3D : ScriptableObject
    {
        public AnimationCurve X;
        public AnimationCurve Y;
        public AnimationCurve Z;
    }
}
