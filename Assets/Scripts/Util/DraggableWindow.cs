using UnityEngine;
using UnityEngine.EventSystems;

namespace DigDig2.Util
{
	public class DraggableWindow : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		[SerializeField] private RectTransform topBar; // Reference to the TopBar for dragging the window
		[SerializeField] private RectTransform bottomLeft; // Reference to BottomLeft for resizing the window
		[SerializeField] private Vector2 minResolution;
		[SerializeField] private Vector2 maxResolution;
		private Canvas canvas; // Reference to the parent canvas
		private bool isDragging;

		private bool isResizing;

		private Vector2 originalLocalPointerPosition;
		private Vector3 originalPointerPosition;
		private Vector3 originalWindowPosition;
		private Vector2 originalWindowSize;
		private Camera uiCamera; // The camera used to render the UI

		private RectTransform windowRect; // Reference to the RectTransform of the DraggableWindow

		private void Awake( )
		{
			windowRect = GetComponent<RectTransform>( );
			canvas = GetComponentInParent<Canvas>( );

			if ( canvas.renderMode is RenderMode.ScreenSpaceCamera or RenderMode.WorldSpace )
				uiCamera = canvas.worldCamera; // Assign the camera if it's Screen Space - Camera or World Space
			else
				Debug.LogError( "This script works with Screen Space - Camera or World Space canvases." );
		}

		public void OnDrag( PointerEventData eventData )
		{
			if ( isResizing )
				ResizeWindow( eventData );
			else if ( isDragging ) DragWindow( eventData );
		}

		public void OnPointerDown( PointerEventData eventData )
		{
			if ( !RectTransformUtility.ScreenPointToWorldPointInRectangle( windowRect, eventData.position, uiCamera, out Vector3 worldPosition ) ) return;

			originalPointerPosition = worldPosition;

			if ( IsPointerInBottomLeft( eventData ) )
			{
				// Start resizing
				isResizing = true;
				originalLocalPointerPosition = eventData.position;
				originalWindowSize = windowRect.sizeDelta;
				originalWindowPosition = windowRect.localPosition;
			} else if ( IsPointerInTopBar( eventData ) )
			{
				// Start dragging
				originalWindowPosition = windowRect.position;
				isDragging = true;
			}
		}

		public void OnPointerUp( PointerEventData eventData )
		{
			// Reset resizing state or perform any other necessary actions
			isResizing = false;
			isDragging = false;
		}

		private bool IsPointerInTopBar( PointerEventData eventData ) =>

			// Check if the pointer is inside the top bar
			RectTransformUtility.RectangleContainsScreenPoint( topBar, eventData.position, uiCamera );

		private bool IsPointerInBottomLeft( PointerEventData eventData ) =>

			// Check if the pointer is inside the bottom-left resize area
			RectTransformUtility.RectangleContainsScreenPoint( bottomLeft, eventData.position, uiCamera );

		private void DragWindow( PointerEventData eventData )
		{
			if ( windowRect == null || canvas == null || uiCamera == null ) return;

			if ( !RectTransformUtility.ScreenPointToWorldPointInRectangle( windowRect, eventData.position, uiCamera, out Vector3 worldPosition ) ) return;

			Vector3 offset = worldPosition - originalPointerPosition;
			windowRect.position = originalWindowPosition + offset;
		}

		private void ResizeWindow( PointerEventData eventData )
		{
			if ( windowRect == null || canvas == null || uiCamera == null ) return;

			Debug.Log( "Resizing window" );

			Vector2 pointerDelta = eventData.position - originalLocalPointerPosition;

			// Adjust the window size based on mouse drag
			var newSize = new Vector2(
				Mathf.Clamp( originalWindowSize.x + pointerDelta.x, minResolution.x, maxResolution.x ), // Prevent shrinking too small horizontally
				Mathf.Clamp( originalWindowSize.y - pointerDelta.y, minResolution.y, maxResolution.y )
			); // Prevent shrinking too small vertically

			// Calculate the new offset for keeping the top-left corner anchored
			var sizeDelta = new Vector3( newSize.x - originalWindowSize.x, newSize.y - originalWindowSize.y, 0 );

			// Adjust the window's position based on the size change
			windowRect.sizeDelta = newSize;
			windowRect.localPosition = originalWindowPosition + new Vector3( sizeDelta.x / 2, -sizeDelta.y / 2, 0 );
		}
	}
}
