using System;
using System.Collections.Generic;
using System.Linq;

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
			UpdateSymbol( );

			if ( InputManager.Instance )
			{
				InputManager.Instance.inputSymbolCycled.AddListener( DisplaySymbol );
				InputManager.Instance.devicesChanged.AddListener( UpdateSymbol );
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

		private void UpdateSymbol( )
		{
			if ( !InputManager.Instance ) return;

			if ( inputActionMap == null || inputAction == null )
			{
				if ( actionPath != "" ) BetterDebug.Log( $"Could not find input action from path \"{actionPath}\".", LogSeverity.Error );
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
            //BetterDebug.Log($"GETTING SYMBOL SPRITES");

			List<Sprite> sprites = new( );
			List<InputControlScheme> inputPlayerControlSchemes = InputManager.Instance.GetInputPlayersControlSchemes( inputPlayerIndex );
            //BetterDebug.Log($"InputPlayerControlSchemes = [{String.Join(", ", inputPlayerControlSchemes.Select(cs => cs.name))}]");
			List<InputDevice> inputPlayerDevices = InputManager.Instance.GetInputPlayersDevices( inputPlayerIndex );
            //BetterDebug.Log($"InputPlayerDevices = [{String.Join(", ", inputPlayerDevices.Select(cs => cs.name))}]");
            
			foreach ( InputBinding inputBinding in inputAction.bindings )
			{
				List<string> inputBindingControlSchemes = inputBinding.groups.Split( ";" ).ToList( );
                //BetterDebug.Log($"inputBindingControlSchemes = [{String.Join(", ", inputBindingControlSchemes)}]");
				bool hasMatchingControlScheme = false;
				InputControlScheme matchingInputControlScheme;

				foreach ( InputControlScheme inputControlScheme in inputPlayerControlSchemes )
				{
					if ( !inputBindingControlSchemes.Contains( inputControlScheme.name ) ) continue;
					hasMatchingControlScheme = true;
					matchingInputControlScheme = inputControlScheme;
					break;
				}
				if ( !hasMatchingControlScheme ) continue;
                //BetterDebug.Log($"FoundMatching Control Scheme [{matchingInputControlScheme}]");

				List<string> addedSymbols = new( );
				InputControlScheme.MatchResult matchResult = matchingInputControlScheme.PickDevicesFrom( inputPlayerDevices );
				foreach ( InputDevice inputDevice in matchResult.devices )
				{
                    //BetterDebug.Log($"[{inputDevice}] is in InputPlayerDevices");
					string inputSymbolCategory = InputManager.Instance.GetInputDeviceSymbolCategory( inputDevice );
                    //BetterDebug.Log($"got inputSymbol Catagory: [{inputSymbolCategory}]");
					string inputSymbolPath = $"{inputSymbolCategory}/{inputBinding.effectivePath}";
					if (addedSymbols.Contains( inputSymbolPath )) continue;
					if ( inputSymbolDictionary.dictionary.TryGetValue( inputSymbolPath, out Sprite sprite ) )
					{
						addedSymbols.Add( inputSymbolPath );
						sprites.Add( sprite );
					}
					else
					{
						BetterDebug.Log( $"Could not find input symbol sprite for \"{inputSymbolPath}\"", LogSeverity.Warning );
					}
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
