using System.Collections;
using UnityEngine;

namespace DigDig2.Entity
{
    public abstract class Dash : MonoBehaviour
    {
        public abstract IEnumerator PerformDash(Vector3 direction, EntityCharacterController entitycontroller);

        public abstract Vector3 GetVelocity();
        
        public abstract bool CanDash();

    }
}