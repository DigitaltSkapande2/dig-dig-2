using System.Linq;
using Cysharp.Threading.Tasks;

using DigDig2.Util;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace DigDig2.Game
{
	public class LoadingScreenManager : Singleton<LoadingScreenManager>
	{
		private UIDocument uiDocument;

		private VisualElement screenCover;
		private VisualElement loadingIndicator;
        

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>( );

            if (screenCover == null) screenCover = uiDocument.rootVisualElement.Query<VisualElement>( "Cover" );
            if (loadingIndicator == null) loadingIndicator = screenCover.Query<VisualElement>( "Indicator" );

            PlayFadeOut( ).Forget( );
        }

        public void LoadScene( int sceneBuildIndex )
		{
			LoadSceneAsync( sceneBuildIndex ).Forget( );
		}

		public async UniTask LoadSceneAsync( int sceneBuildIndex )
		{
			await PlayFadeIn( );
            await SceneManager.LoadSceneAsync( sceneBuildIndex, LoadSceneMode.Single );
			await UniTask.Yield( PlayerLoopTiming.Update );
			await UniTask.WaitForSeconds(3, true);
			await PlayFadeOut( );
		}

		private async UniTask PlayFadeIn( )
		{
			screenCover.style.display = new( DisplayStyle.Flex );
			screenCover.style.visibility = new( Visibility.Visible );
			screenCover.style.opacity = new( 1f );
            
            float duration = screenCover.resolvedStyle.transitionDuration
                .Select(t => t.unit == TimeUnit.Millisecond ? t.value / 1000f : t.value)
                .DefaultIfEmpty(0f)
                .Max();

            await UniTask.WaitForSeconds(duration, true);
			
			loadingIndicator.style.opacity = new( 1f );
		}

		private async UniTask PlayFadeOut( )
		{
			loadingIndicator.style.opacity = new( 0f );
			
            float loadingIndicatorDuration = loadingIndicator.resolvedStyle.transitionDuration
                .Select(t => t.unit == TimeUnit.Millisecond ? t.value / 1000f : t.value)
                .DefaultIfEmpty(0f)
                .Max();
            
            await UniTask.WaitForSeconds(loadingIndicatorDuration, true);
			
			screenCover.style.opacity = new( 0f );

            float duration = screenCover.resolvedStyle.transitionDuration
                .Select(t => t.unit == TimeUnit.Millisecond ? t.value / 1000f : t.value)
                .DefaultIfEmpty(0f)
                .Max();

            await UniTask.WaitForSeconds(duration, true);
			
			screenCover.style.display = new( DisplayStyle.None );
			screenCover.style.visibility = new( Visibility.Hidden );
		}
	}
}