using System.Collections;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

namespace DigDig2.Game
{
    [RequireComponent(typeof(UIDocument))]
    public class EndFadeOut : MonoBehaviour
	{
		[SerializeField] private float distanceTrigger = 0.95f;

		private UIDocument document;
		private SplineContainer spline;

		private VisualElement cover;
		private Image creditsBackground;
		private VisualElement scrollerContainer;
		private VisualElement scroller;
		
		private bool endTriggered = false;

		private void Awake( )
		{
			document = GetComponent<UIDocument>( );
			spline = GetComponentInChildren<SplineContainer>( );
		}

		private void Start( )
		{
			cover = document.rootVisualElement.Query<VisualElement>( "cover" );
			creditsBackground = cover.Query<Image>( "creditsBackground" );
			scrollerContainer = cover.Query<VisualElement>( "scrollerContainer" );
			scroller = scrollerContainer.Query<VisualElement>( "scroller" );
		}

		private void Update()
		{
			if ( endTriggered ) return;

			Vector3 position = Vector3.zero;
			if ( GameManager.Instance.IsMultiplayer )
			{
				GameObject maxCharacterObject = GameManager.Instance.playerMax.characterObject;
				GameObject miniCharacterObject = GameManager.Instance.playerMinis.characterObject;
				position = maxCharacterObject.transform.position + ( maxCharacterObject.transform.position - miniCharacterObject.transform.position ) / 2;
			}
			else { position = GameManager.Instance.PlayerOne.characterObject.transform.position; }

			Vector3 localPosition = ( position - transform.position );
			SplineUtility.GetNearestPoint( spline[ 0 ], localPosition, out float3 point, out float distance );
			Debug.DrawLine( (Vector3)point + transform.position, Vector3.up + (Vector3)point + transform.position, Color.limeGreen );

			cover.style.opacity = new( distance );
			
			if ( distance >= distanceTrigger ) StartCoroutine(TriggerEnd( ));
		}

		private IEnumerator TriggerEnd( )
		{
			endTriggered = true;

			cover.style.opacity = new( 1f );
			yield return new WaitForSecondsRealtime( 0.1f );
			Time.timeScale = 0f;
			
			yield return new WaitForSecondsRealtime( 1f );

			scrollerContainer.style.opacity = new( 1f );

			yield return new WaitForSecondsRealtime( 1f );

			creditsBackground.style.opacity = new( 1f );
			
			yield return new WaitForSecondsRealtime( 4f );

			scrollerContainer.style.top = new( new Length( 100f, LengthUnit.Percent ) );
			scroller.style.top = new( new Length( -100f, LengthUnit.Percent ) );
			
			yield return new WaitForSecondsRealtime( 25f + 1f );

			LoadMainMenu( );
		}

		private void LoadMainMenu( )
		{
			LoadingScreenManager.Instance.LoadScene( 0 );
		}
    }
}
