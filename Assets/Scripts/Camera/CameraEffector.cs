using System;
using System.Collections.Generic;
using DigDig2.CinemaCamera;
using UnityEngine;


namespace DigDig2.CinemaCamera
{
    public class CameraEffector : MonoBehaviour
    {
        public bool isActive;

        [NonSerialized] public Vector3 position;
        [NonSerialized] public bool overridePosition;

        [NonSerialized] public Vector3 rotation;
        [NonSerialized] public bool overrideRotation;

        [NonSerialized] public float frustumSize;
        [NonSerialized] public bool overrideFrustumSize;

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

        private void Start()
        {
            GameCamera.AddCameraEffector(this);
        }
    }
}