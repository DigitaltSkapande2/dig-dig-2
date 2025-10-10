using System.Collections.Generic;
using UnityEngine;

namespace DigDig2
{
    [CreateAssetMenu(fileName = "EntityInfo", menuName = "Scriptable Objects/Entity Info")]
    public class EntityInfo : ScriptableObject
    {
        public List<AttackGroup> attacks;
    }
}
