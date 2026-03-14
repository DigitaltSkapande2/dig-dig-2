using System;
using DigDig2.Game;
using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class PlayerFollowCameraEffector : CameraEffector
    {
        private Action updateAction;

        private void Start()
        {
            updateAction = GameManager.Instance.IsMultiplayer ? MultiPlayerUpdate : SinglePlayerUpdate;
        }

        private void Update( )
        {
            if (GameManager.Instance && GameManager.Instance.PlayerOneCharacter)
            {
                updateAction.Invoke();
            }
		}

        private void MultiPlayerUpdate()
        {
            Vector3 sumPos = Vector3.zero;
            int count = 0;
            foreach (var player in GameManager.Instance.players)
            {
                if (player == null) continue;
                count++;
                sumPos += player.transform.position;
            }
            position = sumPos / count;
        }

        private void SinglePlayerUpdate()
        {
            position = GameManager.Instance.PlayerOneCharacter.transform.position;
        }
	}
}
