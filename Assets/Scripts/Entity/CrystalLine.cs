using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace DigDig2
{
    public class CrystalLine : MonoBehaviour
    {
        Vector3 crystal;
        Vector3 enemy;

        Vector3 controlPoint;

        [SerializeField] float controlPointDefaultPosition;
        [SerializeField] float controlPointResetSpeed;
        [SerializeField] int lineSegmentsPerUnit;
        [SerializeField] float shieldDistance;

        int segments;

        [SerializeField] LineRenderer line;

        public void SetPositions(Vector3 crystal, Vector3 enemy)
        {
            this.crystal = crystal;
            this.enemy = enemy;
        }

        void Start()
        {
            controlPoint = (crystal + enemy) / 2;
        }

        void Update()
        {
            if (Vector3.Distance(crystal, enemy) > 1) segments = (int)Vector3.Distance(crystal, enemy) * lineSegmentsPerUnit;
            else segments = lineSegmentsPerUnit;
            
            line.positionCount = segments + 1;

            controlPoint = Vector3.Lerp(controlPoint, Vector3.Lerp(crystal, enemy, controlPointDefaultPosition), Time.deltaTime * controlPointResetSpeed);

            int index = 0;

            for (int i = 0; i <= segments; i++)
            {
                float value = 1f / segments * i;

                Vector3 p1 = Vector3.Lerp(crystal, controlPoint, value);
                Vector3 p2 = Vector3.Lerp(controlPoint, enemy, value);
                Vector3 p3 = Vector3.Lerp(p1, p2, value);

                if (Vector3.Distance(crystal, p3) < shieldDistance)
                {
                    line.positionCount -= 1;
                    continue;
                }

                if (index == 0)
                {
                    Vector3 offset = p3 - crystal;
                    p3 = crystal + offset.normalized * shieldDistance;

                    line.SetPosition(index, p3);
                    index++;
                    
                    continue;
                }

                line.SetPosition(index, p3);
                index++;
            }
        }
    }
}
