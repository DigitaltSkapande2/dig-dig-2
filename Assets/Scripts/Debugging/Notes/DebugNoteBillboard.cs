using TMPro;

using UnityEngine;

namespace DigDig2.Debugging.Notes
{
	public class DebugNoteBillboard : MonoBehaviour
	{
		[SerializeField] private TMP_Text titleText;
		[SerializeField] private TMP_Text noteText;
		[SerializeField] private TMP_Text authorText;
		[SerializeField] private TMP_Text archivedText;
		[SerializeField] private float unfocusedScale = 0.5f;
		[SerializeField] private Color unfocusedColor = new( 1f, 1f, 1f, 0.5f );
		[SerializeField] private float animationSpeed = 5f;

		private float focusAnimationProgress = 1;
		private float focusAnimationTarget;

		private void Awake( ) { SetFocused( false ); }

		private void Update( )
		{
			if ( Mathf.Approximately( focusAnimationProgress, focusAnimationTarget ) ) return;

			focusAnimationProgress = Mathf.Lerp( focusAnimationProgress, focusAnimationTarget, Time.deltaTime * animationSpeed );

			transform.localScale = Vector3.Lerp( Vector3.one * unfocusedScale, Vector3.one, focusAnimationProgress );
			titleText.color = Color.Lerp( unfocusedColor, Color.white, focusAnimationProgress );
			noteText.color = Color.Lerp( unfocusedColor, Color.white, focusAnimationProgress );
			authorText.color = Color.Lerp( unfocusedColor, Color.white, focusAnimationProgress );
		}

		public void ApplyNoteData( DebugNoteData debugNoteData )
		{
			titleText.text = debugNoteData.title;
			noteText.text = debugNoteData.note;
			authorText.text = $"by {debugNoteData.author}";

			archivedText.gameObject.SetActive( debugNoteData.archived );

			transform.position = debugNoteData.position;
		}

		public void SetFocused( bool newFocusValue ) { focusAnimationTarget = newFocusValue ? 1 : 0; }
	}
}
