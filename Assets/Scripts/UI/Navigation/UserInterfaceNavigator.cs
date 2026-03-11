using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DigDig2.UI.Events;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DigDig2.UI.Navigation
{
	public class UserInterfaceNavigator : MonoBehaviour
	{
		[SerializeField] private string initialNavigationUri = "/";

		[Space( 20 )] [SerializeField]
		private string closedElementClass = "navigation-closed";

		[SerializeField] private string descendantOpenElementClass = "navigation-descendant-open";

		public UnityEvent<string> navigatedTo;
		private NavigationNode hierarchy;
		private string navigationUri = "/";

		public string NavigationUri
		{
			get => navigationUri;
			set => NavigateTo( value );
		}

		public NavigationNode Hierarchy
		{
			get => hierarchy;
			set
			{
				hierarchy = value;
				NavigateTo( initialNavigationUri, true );
			}
		}

		public void NavigateTo( string uri, bool forceRefresh = false )
		{
			if ( navigationUri != uri || forceRefresh )
			{
				navigationUri = uri;
				CloseNode( hierarchy );

				var splitUri = uri.Split( "/" ).ToList( );
				if ( uri == "/" ) splitUri.RemoveAt( 1 );
				splitUri[ 0 ] = hierarchy.name;

				Dictionary<string, Dictionary<string, string>> arguments = new( );
				for ( int index = 0; index < splitUri.Count; index++ )
				{
					string uriPart = splitUri[ index ];
					if ( !uriPart.Contains( "?" ) ) continue;

					var splitUriPart = uriPart.Split( "?" ).ToList( );
					string partName = splitUriPart[ 0 ];
					splitUri[ index ] = partName;
					List<string> splitArguments = new( );
					if ( splitUriPart[ 1 ].Contains( "," ) )
						splitArguments = splitUriPart[ 1 ].Split( "," ).ToList( );
					else
						splitArguments.Add( splitUriPart[ 1 ] );

					Dictionary<string, string> argumentValues = new( );
					foreach ( string combinedArgument in splitArguments )
					{
						var splitArgument = combinedArgument.Split( "=" ).ToList( );
						argumentValues.Add( splitArgument[ 0 ], splitArgument[ 1 ] );
					}

					arguments.Add( partName, argumentValues );
				}

				Debug.Log( $"Navigating to: {uri}" );
				OpenNode( hierarchy, splitUri, arguments );

				navigatedTo.Invoke( uri );
			}
		}

		public void NavigateBack( )
		{
			if ( navigationUri == "/" ) return;

			var splitUri = navigationUri.Split( "/" ).ToList( );
			splitUri.RemoveAt( splitUri.Count - 1 );
			string newUri = string.Join( "/", splitUri );
			if ( newUri == string.Empty ) newUri = "/";
			NavigateTo( newUri );
		}

		private void CloseNode( NavigationNode node )
		{
			if ( node.inputElement != null ) node.inputElement.enabledSelf = false;
			if ( node.element != null )
			{
				node.element.AddToClassList( closedElementClass );
				if ( node.element.style.display != DisplayStyle.None ) StartCoroutine( HideDisplay( node ) );
			}

			if ( node.children == null ) return;

			foreach ( NavigationNode child in node.children ) { CloseNode( child ); }
		}

		private void OpenNode( NavigationNode node, List<string> splitUri, Dictionary<string, Dictionary<string, string>> arguments, int splitIndex = 0 )
		{
			if ( splitIndex >= splitUri.Count ) return;

			if ( node.name == splitUri[ splitIndex ] )
			{
				if ( node.element != null )
				{
					node.element.RemoveFromClassList( closedElementClass );
					node.element.style.display = DisplayStyle.Flex;
				}

				if ( splitIndex >= splitUri.Count - 1 )
				{
					if ( !node.openable )
					{
						NavigateBack( );
						return;
					}

					node.element?.RemoveFromClassList( descendantOpenElementClass );
					if ( node.inputElement != null ) node.inputElement.enabledSelf = true;

					var openedEvent = new NavigatorOpenedEvent( );
					openedEvent.nodeName = node.name;
					openedEvent.arguments = arguments;
					openedEvent.target = node.element;
					node.element?.SendEvent( openedEvent );

					if ( node.focusElement != null )
					{
						if ( !node.focusElement.canGrabFocus )
						{
							Debug.LogError( $"{node.focusElement} was selected as the focusElement but it's not focusable!" );
							return;
						}

						node.focusElement.Focus( );
					}
				} else
				{
					if ( node.inputElement != null ) node.inputElement.enabledSelf = false;
					if ( node.element != null )
					{
						node.element.AddToClassList( descendantOpenElementClass );

						var descendantOpenedEvent = new NavigatorDescendantOpenedEvent( );
						descendantOpenedEvent.nodeName = node.name;
						descendantOpenedEvent.arguments = arguments;
						descendantOpenedEvent.target = node.element;
						node.element.SendEvent( descendantOpenedEvent );
					}
				}

				if ( node.children == null ) return;

				splitIndex++;
				foreach ( NavigationNode child in node.children ) { OpenNode( child, splitUri, arguments, splitIndex ); }
			}
		}

		private IEnumerator HideDisplay( NavigationNode node )
		{
			yield return new WaitForSecondsRealtime( node.closeDuration );

			if ( node.element != null && node.element.ClassListContains( closedElementClass ) ) node.element.style.display = DisplayStyle.None;
		}
	}

	public class NavigationNode
	{
		public readonly List<NavigationNode> children;
		public readonly float closeDuration;
		public readonly VisualElement element;
		public readonly VisualElement focusElement;
		public readonly VisualElement inputElement;
		public readonly string name;
		public readonly bool openable;

		public NavigationNode(
			string name,
			VisualElement element,
			float closeDuration = 0,
			VisualElement focusElement = null,
			VisualElement inputElement = null,
			bool openable = true,
			List<NavigationNode> children = null
		)
		{
			this.name = name;
			this.element = element;
			this.inputElement = inputElement;
			this.focusElement = focusElement;
			this.openable = openable;
			this.closeDuration = closeDuration;
			this.children = children;
		}
	}
}
