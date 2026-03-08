namespace DigDig2.CinemaCamera
{
    public class PlayerFollowCameraEffector : CameraEffector
    {
        void Update()
        {
            if (GameManager.Instance && GameManager.Instance.PlayerOneCharacter) position = GameManager.Instance.PlayerOneCharacter.transform.position;
        }
    }
}

