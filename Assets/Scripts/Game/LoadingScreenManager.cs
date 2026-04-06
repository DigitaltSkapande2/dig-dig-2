using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using DigDig2.Debugging;
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

		

		protected override void Awake( )
		{
			base.Awake( );

			uiDocument = GetComponent<UIDocument>( );

			screenCover = uiDocument.rootVisualElement.Query<VisualElement>( "Cover" );
			loadingIndicator = screenCover.Query<VisualElement>( "Indicator" );

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

            await UniTask.WaitForSeconds(duration);
			
			loadingIndicator.style.opacity = new( 1f );
		}

		private async UniTask PlayFadeOut( )
		{
			loadingIndicator.style.opacity = new( 0f );
			
            float loadingIndicatorDuration = loadingIndicator.resolvedStyle.transitionDuration
                .Select(t => t.unit == TimeUnit.Millisecond ? t.value / 1000f : t.value)
                .DefaultIfEmpty(0f)
                .Max();
            
            await UniTask.WaitForSeconds(loadingIndicatorDuration);
			
			screenCover.style.opacity = new( 0f );

            float duration = screenCover.resolvedStyle.transitionDuration
                .Select(t => t.unit == TimeUnit.Millisecond ? t.value / 1000f : t.value)
                .DefaultIfEmpty(0f)
                .Max();

            await UniTask.WaitForSeconds(duration);
			
			screenCover.style.display = new( DisplayStyle.None );
			screenCover.style.visibility = new( Visibility.Hidden );
		}
	}
}