using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace DigDig2
{
    public class SwingCollisionCurve : MonoBehaviour
    {
        [SerializeField] AttackData attackData;

        Transform box;

        Vector3 dir;

        float t;
        bool active;

        public void Attack(AttackData data)
        {
            attackData = data;

            t = 0;
            active = true;

            Vector3 pos = new Vector3(attackData.X.Evaluate(0), attackData.Y.Evaluate(0), attackData.Z.Evaluate(0));
            Vector3 stepPos = new Vector3(attackData.X.Evaluate(attackData.step), attackData.Y.Evaluate(attackData.step), attackData.Z.Evaluate(attackData.step));

            dir = (stepPos - pos).normalized;

            box = Instantiate(attackData.hitbox, transform.position + pos, quaternion.identity, transform).transform;
            box.forward = dir;
        }

        void FixedUpdate()
        {
            if (!active) return;

            t += 1 / attackData.attackTime * Time.deltaTime;
            float position = attackData.speed.Evaluate(t);

            float X = attackData.X.Evaluate(position);
            float Y = attackData.Y.Evaluate(position);
            float Z = attackData.Z.Evaluate(position);

            float stepX = attackData.X.Evaluate(position + attackData.step);
            float stepY = attackData.Y.Evaluate(position + attackData.step);
            float stepZ = attackData.Z.Evaluate(position + attackData.step);

            Vector3 pos = transform.right * X + transform.up * Y + transform.forward * Z;
            Vector3 stepPos = transform.right * stepX + transform.up * stepY + transform.forward * stepZ;

            if (position + attackData.step < 1)
            {
                dir = (stepPos - pos).normalized;
            }

            box.position = transform.position + pos;
            box.forward = dir;

            if (t > 1)
            {
                active = false;
                Destroy(box);
            }
        }

        void Update()
        {
            if (!active && Input.GetKeyDown(KeyCode.J))
            {
                UnityEngine.Debug.Log("goon");
                Attack(attackData);
            }
        }
    }
}
