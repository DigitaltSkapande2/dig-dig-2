using System.Collections.Generic;
using DigDig2.CinemaCamera;
using UnityEngine;


namespace DigDig2.CinemaCamera
{
    public class CameraEffector : MonoBehaviour
    {
        public bool isActive;

        public Vector3 position;
        public bool overridePosition;

        public Vector3 rotation;
        public bool overrideRotation;

        public float frustumSize;
        public bool overrideFrustumSize;

        [SerializeField] private int priorityLevel;
        public int PriorityLevel
        {
            get
            {
                return priorityLevel;
            }
            set
            {
                GameCamera.RemoveCameraEffector(this);

                priorityLevel = value;

                GameCamera.AddCameraEffector(this);
            }
        }

        public CameraEffector()
        {
            
        }
    }
}