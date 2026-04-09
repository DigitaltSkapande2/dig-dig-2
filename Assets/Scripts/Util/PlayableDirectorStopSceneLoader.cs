using DigDig2.Game;
using UnityEngine;
using UnityEngine.Playables;

namespace DigDig2.Util
{
    public class PlayableDirectorStopSceneLoader : MonoBehaviour
    {
        [SerializeField] private PlayableDirector targetPlayable;
        [SerializeField] private int sceneIndexToLoad;

        private void Start()
        {
            targetPlayable.stopped += OnStopped;
        }

        private void OnStopped(PlayableDirector playable)
        {
            
            LoadingScreenManager.Instance.LoadScene(sceneIndexToLoad);
        }

        private void OnDisable()
        {
            targetPlayable.stopped -= OnStopped;
        }
    }
}