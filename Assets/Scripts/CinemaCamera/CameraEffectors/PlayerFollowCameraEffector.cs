using DigDig2.Game;
using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class PlayerFollowCameraEffector : CameraEffector
	{
		private void Update( )
        {
            if (GameManager.Instance && GameManager.Instance.PlayerOneCharacter)
            {
                Vector3 sumPos = Vector3.zero;
                foreach (var player in GameManager.Instance.players)
                {
                    if (player == null) continue;
                    sumPos += player.transform.position;
                }
                position = sumPos / GameManager.Instance.players.Length;
            }
		}
	}
}
