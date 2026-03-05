using UnityEngine;
using UnityEngine.Splines;

namespace DigDig2.CinemaCamera
{
    public class CameraCurveEffector : TriggerZoneCameraEffectorBase
    {
        [SerializeField] private SplineContainer splineContainer;

        private Spline projectionSpline; // The spline to match the Player
        private Spline dataSpline; // the spline to get the camera data from


        private new void Start()
        {
            projectionSpline = splineContainer.Splines[0];
            dataSpline = splineContainer.Splines[1];
            if (projectionSpline == null || dataSpline == null)
            {
                Debug.LogError("CameraCurveEffector requires a SplineContainer with two splines.");
            }
        }

        protected override void WhileActiveUpdate()
        {

        }

        protected override void OnZoneEnter()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnZoneExit()
        {
            throw new System.NotImplementedException();
        }

        // private float GetClosestPositionOnSpline(Spline spline, Vector3 point, int iterations = 10)
        // {
        //     float currentT = 0.5f; //Start in the middle of the spline
        //     for (int i = 0; i < iterations; i++)
        //     {
        //         float step = 1f / Mathf.Pow(2, i + 1); // Decrease step size each iteration
        //         if (Vector2.Distance(point, spline.Evaluate(currentT - step))
        //             > Vector2.Distance(point, spline.Evaluate(currentT + step)))
        //         {
        //             currentT += step;
        //         }
        //         else if (Vector2.Distance(point, spline.Evaluate(currentT, out evalData))
        //             > Vector2.Distance(point, spline.Evaluate(currentT - step, out evalData)))
        //         {
        //             currentT -= step;
        //         }

        //     }
        // }

    }
}
