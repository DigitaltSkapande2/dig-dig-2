using UnityEngine;

namespace DigDig2.CinemaCamera 
{
    public class PlayerTargetCameraEffector : CameraEffector
    {
        void Update()
        {
            if (GameManager.Instance.CurrentCharacter && GameManager.Instance.CurrentCharacter == gameObject) position = GameManager.Instance.CurrentCharacter.transform.position;
        } 
    }
}

