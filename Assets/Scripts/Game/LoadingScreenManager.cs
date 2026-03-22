using System.Collections;

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

			PlayFadeOut( );
		}

		public void LoadScene( int sceneBuildIndex )
		{
			StartCoroutine( LoadSceneAsync( sceneBuildIndex ) );
		}

		public IEnumerator LoadSceneAsync( int sceneBuildIndex )
		{
			AsyncOperation operation = SceneManager.LoadSceneAsync( sceneBuildIndex, LoadSceneMode.Single );
			if ( operation == null ) yield break;

			yield return PlayFadeIn( );
			yield return new WaitWhile( ( ) => !operation.isDone );
			yield return PlayFadeOut( );
		}

		private IEnumerator PlayFadeIn( )
		{
			screenCover.style.display = new( DisplayStyle.Flex );
			screenCover.style.visibility = new( Visibility.Visible );
			screenCover.style.opacity = new( 1f );
			
			yield return screenCover.resolvedStyle.transitionDuration;
			
			loadingIndicator.style.opacity = new( 1f );
		}

		private IEnumerator PlayFadeOut( )
		{
			loadingIndicator.style.opacity = new( 0f );
			
			yield return loadingIndicator.resolvedStyle.transitionDuration;
			
			screenCover.style.opacity = new( 0f );

			yield return screenCover.resolvedStyle.transitionDuration;
			
			screenCover.style.display = new( DisplayStyle.None );
			screenCover.style.visibility = new( Visibility.Hidden );
		}
	}
}
