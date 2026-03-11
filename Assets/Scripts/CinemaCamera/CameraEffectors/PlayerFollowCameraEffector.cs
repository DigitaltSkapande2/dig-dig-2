using DigDig2.Game;

namespace DigDig2.CinemaCamera.CameraEffectors {
	public class PlayerFollowCameraEffector : CameraEffector {
		private void Update( ) {
			if ( GameManager.Instance && GameManager.Instance.PlayerOneCharacter ) position = GameManager.Instance.PlayerOneCharacter.transform.position;
		}
	}
}
