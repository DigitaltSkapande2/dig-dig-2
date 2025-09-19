using UnityEngine;
using UnityEngine.Splines;

namespace DigDig2.CinemaCamera
{
    public class CameraCinemaCurve : CameraEffector
    {
        [SerializeField] SplineContainer splineContainer;

        private Spline spline;

        void Start()
        {
            spline = splineContainer.Splines[0];
        }

        void Update()
        {
            
        }
    }
}
