using System;
using DigDig2.Game;
using UnityEngine;

namespace DigDig2.CinemaCamera.CameraEffectors
{
	public class PlayerFollowCameraEffector : CameraEffector
    {
        private Action updateAction;

        private GameManager gameManager;
        
        
        protected void OnEnable()
        {
            base.Start();
            gameManager = GameManager.Instance;
            updateAction = gameManager.IsMultiplayer ? MultiPlayerUpdate : SinglePlayerUpdate;
        }

        private void Update( )
        {
            if (gameManager && gameManager.PlayerOne)
            {
                updateAction.Invoke();
            }
		}

        private void MultiPlayerUpdate()
        {
            Vector3 sumPos = Vector3.zero;
            int count = 0;
            foreach (var player in GameManager.Instance.PlayerCharacterObjects)
            {
                if (player == null) continue;
                count++;
                sumPos += player.transform.position;
            }
            if (count <= 0) return;
            targetPosition = sumPos / count;
        }

        private void SinglePlayerUpdate()
        {
            targetPosition = GameManager.Instance.PlayerOne.characterObject.transform.position;
        }
	}
}
