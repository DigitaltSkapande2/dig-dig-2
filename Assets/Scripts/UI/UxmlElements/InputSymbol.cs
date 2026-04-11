using System.Collections.Generic;

using DigDig2.Debugging;
using DigDig2.Input;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace DigDig2.UI.UxmlElements
{
	[UxmlElement]
	public partial class InputSymbol : VisualElement
	{
		private const string INPUT_SYMBOL_DICTIONARY_PATH = "UI/InputSymbols/InputSymbolDictionary";

		private bool displayDescriptorLabel = true;
		private string actionPath = "";
		private int inputPlayerIndex = -1;

		private InputActionMap inputActionMap;
		private InputAction inputAction;

		private Image symbolImage;
		private Label descriptorLabel;

		private InputSymbolDictionary inputSymbolDictionary;

		private List<Sprite> inputSymbols = new( );
		
		public InputSymbol( )
		{
			style.flexDirection = new( FlexDirection.Row );
			
			Add( symbolImage = new( ) );
			symbolImage.style.height = new( new Length( 100f, LengthUnit.Percent ) );
			Add( descriptorLabel = new( ) );
			descriptorLabel.style.unityTextAlign = new( TextAnchor.MiddleLeft );
			descriptorLabel.style.marginLeft = new( new Length( 20, LengthUnit.Pixel ) );
			descriptorLabel.style.display = new( displayDescriptorLabel ? DisplayStyle.Flex : DisplayStyle.None );

			UpdateAction( );
			UpdateInputPlayerIndex( );
			UpdateSymbol( );

			if ( InputManager.Instance )
			{
				InputManager.Instance.inputSymbolCycled.AddListener( DisplaySymbol );
			}
		}

		private void DisplaySymbol( int cycle )
		{
			if ( inputSymbols.Count > 0 )
			{
				int currentSymbolIndex = cycle % inputSymbols.Count;
				Sprite symbolSprite = inputSymbols[ currentSymbolIndex ];
				float aspectRatio = symbolSprite.rect.width / symbolSprite.rect.height;
				symbolImage.style.aspectRatio = aspectRatio;
				symbolImage.sprite = symbolSprite;
			}
			else
			{
				symbolImage.sprite = null;
			}
		}

		private void UpdateAction( )
		{
			if ( !InputManager.Instance ) return;
			
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
			
			foreach ( InputAction action in inputActionMap.actions)
			{
				if ( action.name != actionName ) continue;

				inputAction = action;
			}
			
			if ( inputAction == null ) return;
		}

		private void UpdateInputPlayerIndex( )
		{
			UpdateSymbol( );
		}

		private void UpdateSymbol( )
		{
			if ( !InputManager.Instance ) return;

			if ( inputActionMap == null || inputAction == null )
			{
				BetterDebug.Log( $"Could not find input action from path \"{actionPath}\".", LogSeverity.Error );
				return;
			};
			
			descriptorLabel.text = inputAction.name;
			inputSymbols = GetSymbolSprites( );
			DisplaySymbol( InputManager.Instance.InputSymbolCycle );
		}

		private List<Sprite> GetSymbolSprites( )
		{
			if ( !InputManager.Instance ) return new( );
			if ( !inputSymbolDictionary ) inputSymbolDictionary = Resources.Load<InputSymbolDictionary>( INPUT_SYMBOL_DICTIONARY_PATH );
			if ( !inputSymbolDictionary ) return new( );

			List<Sprite> sprites = new( );
			List<InputDevice> inputPlayerDevices = InputManager.Instance.GetInputPlayersDevices( inputPlayerIndex );
			foreach ( InputControl inputControl in inputAction.controls )
			{
				if ( !inputPlayerDevices.Contains( inputControl.device ) ) continue;

				string inputSymbolCategory = InputManager.Instance.GetInputDeviceSymbolCategory( inputControl.device );
				string inputSymbolPath = $"{inputSymbolCategory}/{inputControl.displayName}";
				if ( inputSymbolDictionary.dictionary.TryGetValue( inputSymbolPath, out Sprite sprite ) )
				{
					sprites.Add( sprite );
				}
				else
				{
					BetterDebug.Log( $"Could not find input symbol sprite for \"{inputSymbolPath}\"", LogSeverity.Warning );
				}
			}

			return sprites;
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

		[UxmlAttribute] public bool DisplayDescriptorLabel
		{
			get => displayDescriptorLabel;
			set
			{
				displayDescriptorLabel = value;
				descriptorLabel.style.display = new( displayDescriptorLabel ? DisplayStyle.Flex : DisplayStyle.None );
			}
		}
	}
}
