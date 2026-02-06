using UnityEngine;

namespace DigDig2.CinemaCamera 
{
    public class PlayerFollowCameraEffector : CameraEffector
    {
        void Update()
        {
            if (GameManager.Instance.CurrentCharacter) position = GameManager.Instance.CurrentCharacter.transform.position;
        } 
    }
}

