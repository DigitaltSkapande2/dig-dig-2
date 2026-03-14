using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DigDig2.Input;

using UnityEngine;
using UnityEngine.UI;

namespace DigDig2.Debugging.Menu
{
	public class DebugMenu : MonoBehaviour
	{
		[Header( "Object ref" )]
		[SerializeField] private GameObject debugMenuGraphics;

		[SerializeField] private RectTransform debugMenuContent;
		[SerializeField] private GameObject debugMenuElementPrefab;

		private readonly List<DebugMenuElement> debugMenuElements = new( );

		public void OpenMenu( )
		{
			SetMenuActive( true );
			UpdateMenuElements( );
		}

		public void CloseMenu( ) { SetMenuActive( false ); }

		private void OnInputDebugMenuOpenDebugMenu( InputInfo _ ) { OpenMenu( ); }
		private void OnInputDebugMenuCloseDebugMenu( InputInfo _ ) { CloseMenu( ); }

		private void SetMenuActive( bool state ) { debugMenuGraphics.SetActive( state ); }

		private void UpdateMenuElements( )
		{
			ClearMenuElements( );
			GenerateMenuElements( );
			LayoutRebuilder.MarkLayoutForRebuild( debugMenuContent );
		}

		private void GenerateMenuElements( )
		{
			Debug.Log( "Generating Debug Menu Elements..." );

			// Find all objects in the scene
			MonoBehaviour[ ] allBehaviours = FindObjectsByType<MonoBehaviour>( FindObjectsInactive.Include, FindObjectsSortMode.None );

			var nonDebugableBehaviourTypes = new List<Type>( );

			foreach ( MonoBehaviour behaviour in allBehaviours )
			{
				Type behaviourType = behaviour.GetType( );
				if ( nonDebugableBehaviourTypes.Contains( behaviourType ) ) continue;

				// Check the class for [Debug] attribute
				if ( Attribute.IsDefined( behaviourType, typeof( DebugAttribute ) ) )
				{
					var debugAttr = (DebugAttribute)Attribute.GetCustomAttribute( behaviourType, typeof( DebugAttribute ) );

					// Find fields marked with [DebugSerialized]
					FieldInfo[ ] debugFields = behaviourType.GetFields( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance )
							.Where( f => Attribute.IsDefined( f, typeof( DebugSerializedAttribute ) ) )
							.ToArray( )
						;

					// Spawn menu element for this behaviour
					DebugMenuElement element = SpawnMenuElement( behaviour, debugFields, debugAttr.IsToggleable );
					debugMenuElements.Add( element );
				}
				else
					nonDebugableBehaviourTypes.Add( behaviourType );
			}
		}

		private DebugMenuElement SpawnMenuElement( MonoBehaviour behaviour, FieldInfo[ ] debugFields, DebugMenuToggleable toggleable )
		{
			DebugMenuElement elementComponent = Instantiate( debugMenuElementPrefab, debugMenuContent ).GetComponent<DebugMenuElement>( );
			elementComponent.Initialize( behaviour, debugFields, toggleable );
			return elementComponent;
		}

		private void ClearMenuElements( )
		{
			foreach ( DebugMenuElement element in debugMenuElements ) { Destroy( element.gameObject ); }

			debugMenuElements.Clear( );
		}
	}
}
