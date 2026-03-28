using DigDig2.Debugging;

using Unity.Properties;

#if UNITY_EDITOR
using UnityEditor;


using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DigDig2.UI.UxmlElements
{
	[UxmlElement]
	public partial class InputSymbol : VisualElement
	{
		private string actionPath = "";
		private int inputPlayerIndex;

		private InputActionMap inputActionMap;
		private InputAction inputAction;

		private Image symbolImage;
		private Label descriptorLabel;
		
		public InputSymbol( )
		{
			style.flexDirection = new( FlexDirection.Row );
			
			Add( symbolImage = new( ) );
			symbolImage.style.marginRight = new( new Length( 20, LengthUnit.Pixel ) );
			Add( descriptorLabel = new( ) );

			UpdateAction( );
			UpdateInputPlayerIndex( );
			UpdateSymbol( );
		}

		private void UpdateAction( )
		{
			BetterDebug.Log( "Updating action" );
			
			string[] splitActionPath = actionPath.Split( "/" );
			if ( splitActionPath.Length < 2 ) return;
			
			string actionMapName = splitActionPath[ 0 ];
			string actionName = splitActionPath[ 1 ];

			inputActionMap = null;
			inputAction = null;
			
			foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps )
			{
				if ( actionMap.name != actionMapName ) continue;

				inputActionMap = actionMap;
				break;
			}
			
			if ( inputActionMap == null ) return;
			
			BetterDebug.Log( "Found action map" );
			
			foreach ( InputAction action in inputActionMap.actions)
			{
				if ( action.name != actionName ) continue;

				inputAction = action;
			}
			
			if ( inputAction == null ) return;
			
			BetterDebug.Log( "Found action" );
		}

		private void UpdateInputPlayerIndex( )
		{
			
		}

		private void UpdateSymbol( )
		{
			if ( inputActionMap != null && inputAction != null )
			{
				descriptorLabel.text = actionPath;
				symbolImage.sprite = GetSymbolSprite( );
			}
		}

		private Sprite GetSymbolSprite( )
		{
			return AssetDatabase.LoadAssetAtPath<Sprite>( "Assets/Textures/UI/InputSymbols/InputSymbols.png" );
		}
		
		[UxmlAttribute] public string ActionPath
		{
			get => actionPath;
			set
			{
				actionPath = value;
				UpdateAction( );
				UpdateSymbol( );
			}
		}

		[UxmlAttribute] public int InputPlayerIndex
		{
			get => inputPlayerIndex;
			set
			{
				inputPlayerIndex = value;
				UpdateInputPlayerIndex( );
				UpdateSymbol( );
			}
		}
	}
}

#endif
