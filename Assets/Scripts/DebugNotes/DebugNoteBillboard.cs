using TMPro;
using UnityEngine;

public class DebugNoteBillboard : MonoBehaviour
{
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private TMP_Text noteText;
	[SerializeField] private TMP_Text authorText;
	[SerializeField] private float unfocusedScale = 0.5f;
	[SerializeField] private Color unfocusedColor = new(1f, 1f, 1f, 0.5f);
	[SerializeField] private float animationSpeed = 5f;

	private float focusAnimationTarget = 0;
	private float focusAnimationProgress = 1;



	private void Awake()
	{
		SetFocused(false);
	}

	private void Update()
	{
		if (focusAnimationProgress != focusAnimationTarget)
		{
			focusAnimationProgress = Mathf.Lerp(focusAnimationProgress, focusAnimationTarget, Time.deltaTime * animationSpeed);

			transform.localScale = Vector3.Lerp(Vector3.one * unfocusedScale, Vector3.one, focusAnimationProgress);
			titleText.color = Color.Lerp(unfocusedColor, Color.white, focusAnimationProgress);
			noteText.color = Color.Lerp(unfocusedColor, Color.white, focusAnimationProgress);
			authorText.color = Color.Lerp(unfocusedColor, Color.white, focusAnimationProgress);
		}
	}

	public void ApplyNoteData(DebugNoteData debugNoteData)
	{
		titleText.text = debugNoteData.title;
		noteText.text = debugNoteData.note;
		authorText.text = $"by {debugNoteData.author}";
	}

	public void SetFocused(bool newFocusValue)
	{
		focusAnimationTarget = newFocusValue ? 1 : 0;
	}
}
