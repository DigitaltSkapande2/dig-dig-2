using DigDig2;
using Mirror.BouncyCastle.Math.EC.Rfc7748;
using UnityEngine;



public class CurveDrawer : MonoBehaviour
{
    [SerializeField] int positions;
    [SerializeField] AttackData curve;


    LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
    void FixedUpdate()
    {
        lineRenderer.positionCount = positions;

        for (int i = 0; i < positions; i++)
        {
            float lerp = 1f / positions * i;
            float curveX = curve.X.Evaluate(lerp);
            float curveY = curve.Y.Evaluate(lerp);
            float curveZ = curve.Z.Evaluate(lerp);

            Vector3 pos = transform.right * curveX + transform.up * curveY + transform.forward * curveZ;

            lineRenderer.SetPosition(i, transform.position + pos);
        }
    }
}
