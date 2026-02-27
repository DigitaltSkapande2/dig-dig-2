using UnityEngine;

namespace DigDig2.CinemaCamera
{
    public class PlayerFollowCameraEffector : CameraEffector
    {
        void Update()
        {
            if (GameManager.Instance.LocalPlayerObj) position = GameManager.Instance.LocalPlayerObj.transform.position;
        }
    }
}
