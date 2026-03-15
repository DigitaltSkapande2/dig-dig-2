using System.Collections.Generic;
using System.Linq;

using DigDig2.Debugging;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace DigDig2.Input.InputPlayerManagers
{
	public class ControlSchemeDevicesInputPlayerManager : InputPlayerManager
	{
		[SerializeField] private List<string> supportedControlSchemes = new( );

		private List<InputDevice> unusedDevices = new( );

		private void Awake( )
		{
			List<string> invalidSupportedControlSchemes = new( );
			invalidSupportedControlSchemes.AddRange( supportedControlSchemes );
			foreach ( InputControlScheme controlScheme in InputSystem.actions.controlSchemes ) { invalidSupportedControlSchemes.Remove( controlScheme.name ); }

			if ( invalidSupportedControlSchemes.Count > 0 ) BetterDebug.Log( $"Invalid supported control schemes: {string.Join( ", ", invalidSupportedControlSchemes )}. Please add them to the InputActionAsset.", LogSeverity.Error );
		}

		public override List<InputPlayer> GenerateList( )
		{
			if ( hasGeneratedList ) throw new( "Can only generate list once, create a new InputPlayerManager component to generate a new list." );
			hasGeneratedList = true;

			unusedDevices = InputSystem.devices.ToArray( ).ToList( );
			
			return ReconcileInputPlayers( );
		}

		private List<InputPlayer> ReconcileInputPlayers( List<InputPlayer> inputPlayers = null )
		{
			inputPlayers ??= new( );
			
			while ( true )
			{
				int successfulConstructions = 0;
				foreach ( InputControlScheme controlScheme in InputSystem.actions.controlSchemes )
				{
					if ( !supportedControlSchemes.Contains( controlScheme.name ) ) continue;

					InputControlScheme.MatchResult deviceSearchMatchResult = controlScheme.PickDevicesFrom( unusedDevices );
					if ( deviceSearchMatchResult.hasMissingRequiredDevices ) continue;

					InputPlayer controlSchemeInputPlayer = new( );
					controlSchemeInputPlayer.connectedDevices.AddRange( deviceSearchMatchResult.devices.ToArray( ) );
                    controlSchemeInputPlayer.name = controlScheme.name;
                    
                    List<InputActionMap> newListOfActionMaps = new List<InputActionMap>();
                    foreach (var actionMap in InputSystem.actions.actionMaps)
                    {
                        var newActionMap = actionMap.Clone();
                        newActionMap.devices = new ReadOnlyArray<InputDevice>(controlSchemeInputPlayer.connectedDevices.ToArray());
                        newActionMap.Enable();
                        newActionMap.actionTriggered += controlSchemeInputPlayer.OnActionTriggered;
                        newListOfActionMaps.Add(newActionMap);
                    }
                    controlSchemeInputPlayer.actionMaps.AddRange(newListOfActionMaps);

					inputPlayers.Add( controlSchemeInputPlayer );

					successfulConstructions++;
					foreach ( InputDevice device in deviceSearchMatchResult.devices ) { unusedDevices.Remove( device ); }

					inputPlayerAdded.Invoke( controlSchemeInputPlayer, inputPlayers.IndexOf( controlSchemeInputPlayer ) );
					BetterDebug.Log( $"Generated InputPlayer from control scheme \"{controlScheme.name}\" with \"{string.Join( ", ", deviceSearchMatchResult.devices.ToArray( ).ToList( ).Select( device => device.displayName ) )}\" as the devices." );
				}

				if ( successfulConstructions <= 0 ) break;
			}

			return inputPlayers;
		}

		public override List<InputPlayer> AddDevice( List<InputPlayer> inputPlayers, InputDevice device )
		{
			bool deviceUnused = true;
			foreach ( InputPlayer inputPlayer in inputPlayers )
			{
				if ( !inputPlayer.connectedDevices.Contains( device ) ) continue;
				
				bool fullControlSchemeFound = false;
				foreach ( InputControlScheme controlScheme in InputSystem.actions.controlSchemes )
				{
					InputControlScheme.MatchResult deviceSearchMatchResult = controlScheme.PickDevicesFrom( inputPlayer.connectedDevices );
					if ( deviceSearchMatchResult.hasMissingRequiredDevices ) continue;

					fullControlSchemeFound = true;
					break;
				}

				if ( fullControlSchemeFound )
				{
					inputPlayer.active = true;
					inputPlayerReconnected.Invoke( inputPlayer, inputPlayers.IndexOf( inputPlayer ) );
				}
				
				deviceUnused = false;
				break;
			}
			
			if ( deviceUnused ) unusedDevices.Add( device );
			return ReconcileInputPlayers( inputPlayers );
		}

		public override List<InputPlayer> RemoveDevice( List<InputPlayer> inputPlayers, InputDevice device )
		{
			foreach ( InputPlayer inputPlayer in inputPlayers )
			{
				if ( !inputPlayer.connectedDevices.Contains( device ) ) continue;

				inputPlayer.active = false;
				inputPlayerDisconnected.Invoke( inputPlayer, inputPlayers.IndexOf( inputPlayer ) );
			}

			return inputPlayers;
		}
	}
}
