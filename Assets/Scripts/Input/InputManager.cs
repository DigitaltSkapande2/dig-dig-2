using System.Collections.Generic;
using System.Linq;

using DigDig2.Debugging;
using DigDig2.Util;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Input
{
	// TODO:
	// Fix memory leaks (check unity console) - Maybe fixed?
	// Add support for multiple action maps on InputModules
	// Fix input "lag" issue when filtering InputPlayer
	public class InputManager : Singleton<InputManager>
	{
		public InputPlayerManager InputPlayerManager
		{
			get => inputPlayerManager;
		}
		
		public InputContext CurrentInputContext
		{
			get => currentInputContext;
			set
			{
				currentInputContext = value;
				RefreshInputModuleRegistrations( );
			}
		}
		
		[Header( "Input Manager" )]
		[SerializeField] private InputPlayerManager inputPlayerManager;

		[SerializeField] [OnChangedCall( "RefreshInputModuleRegistrations" )]
		private InputContext currentInputContext;

		private readonly List<InputModule> activeInputModules = new( );
		private readonly Dictionary<InputPlayer, Dictionary<string, List<InputModule>>> prioritizedInputModules = new( );

		private readonly List<string> validActionMapNames = new( );

		private List<InputPlayer> inputPlayers = new( );

		protected override void Awake( )
		{
			base.Awake( );

			if ( !inputPlayerManager )
			{
				BetterDebug.Log( "No input player generator given, please create one!", LogSeverity.Error );
				return;
			}

			RefreshValidActionMapNames( );

			SetupDeviceMonitoring( );
			SetupInputMonitoring( );
		}

		private void Start( )
		{
			inputPlayers = inputPlayerManager.GenerateList( );
			BetterDebug.Log( $"{inputPlayers.Count} InputPlayer(s) active!" );

			RefreshInputModulePrioritizationLists( );
			RefreshInputModuleRegistrations( );
		}

		private void OnEnable( )
		{
			SetupDeviceMonitoring( );
			SetupInputMonitoring( );
		}

		private void OnDisable( ) { Cleanup( ); }

		private void Cleanup( )
		{
			CleanupDeviceMonitoring( );
			CleanupInputMonitoring( );
		}

		private void RefreshValidActionMapNames( )
		{
			validActionMapNames.Clear( );
			foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps ) { validActionMapNames.Add( actionMap.name ); }
		}

		#region Input Contexting

		// Has to be public for the OnChangedCall attribute to work.
		public void RefreshInputModuleRegistrations( )
		{
			BetterDebug.Log( "Refreshing InputModule registrations." );
			foreach ( InputModule inputModule in FindObjectsByType<InputModule>( FindObjectsInactive.Exclude, FindObjectsSortMode.None ) ) { inputModule.RefreshRegistration( ); }
		}

		#endregion

		#region Input Monitoring

		private void SetupInputMonitoring( )
		{
			foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps ) { actionMap.actionTriggered += OnActionTriggered; }
		}

		private void CleanupInputMonitoring( )
		{
			foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps ) { actionMap.actionTriggered -= OnActionTriggered; }
		}

		private void OnActionTriggered( InputAction.CallbackContext context )
		{
			for ( int inputPlayerIndex = 0; inputPlayerIndex < inputPlayers.Count; inputPlayerIndex++ )
			{
				InputPlayer inputPlayer = inputPlayers[ inputPlayerIndex ];
				if ( !inputPlayer.connectedDevices.Contains( context.control.device ) ) continue;
				if ( !inputPlayer.active ) continue;

				foreach ( InputModule prioritizedInputModule in prioritizedInputModules[ inputPlayer ][ context.action.actionMap.name ] )
				{
					if ( prioritizedInputModule.AllowedInputPlayerIndex == -1 || prioritizedInputModule.AllowedInputPlayerIndex == inputPlayerIndex ) prioritizedInputModule.SendInput( context, inputPlayer, inputPlayerIndex );
				}
			}
		}

		#endregion

		#region Input Devices & Players

		private void SetupDeviceMonitoring( ) { InputSystem.onDeviceChange += OnDeviceChanged; }

		private void CleanupDeviceMonitoring( ) { InputSystem.onDeviceChange -= OnDeviceChanged; }

		private void OnDeviceChanged( InputDevice device, InputDeviceChange change )
		{
			inputPlayers = change switch
			{
				InputDeviceChange.Added => inputPlayerManager.AddDevice( inputPlayers, device ),
				InputDeviceChange.Removed => inputPlayerManager.RemoveDevice( inputPlayers, device ),
				_ => inputPlayers
			};

			if ( change == InputDeviceChange.Added || change == InputDeviceChange.Removed )
			{
				int activeInputPlayers = 0;
				foreach ( InputPlayer inputPlayer in inputPlayers )
				{
					if ( inputPlayer.active ) activeInputPlayers++;
				}
				
				BetterDebug.Log( $"Devices changed, {activeInputPlayers} InputPlayer(s) active." );
				RefreshInputModulePrioritizationLists( );
			}
		}

		#endregion

		#region Input Modules & Priority Sorting

		private void RefreshInputModulePrioritizationLists( )
		{
			prioritizedInputModules.Clear( );
			foreach ( InputPlayer inputPlayer in inputPlayers )
			{
				Dictionary<string, List<InputModule>> inputPlayerPrioritizedInputModules = new( );
				foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps ) { inputPlayerPrioritizedInputModules.Add( actionMap.name, new( ) ); }

				prioritizedInputModules.Add( inputPlayer, inputPlayerPrioritizedInputModules );
			}

			foreach ( InputActionMap actionMap in InputSystem.actions.actionMaps ) { RefillAllPriorityLists( actionMap.name ); }
		}

		public void RegisterInputModule( InputModule module )
		{
			if ( !validActionMapNames.Contains( module.ActionMapName ) )
			{
				BetterDebug.Log( $"\"{module.ActionMapName}\" is not a valid InputActionMap, {module.name}'s InputModule did not get registered.", LogSeverity.Error );
				return;
			}

			if ( activeInputModules.Contains( module ) )
			{
				BetterDebug.Log( $"{module.name}'s InputModule was already registered.", LogSeverity.Warning );
				return;
			}

			activeInputModules.Add( module );

			foreach ( KeyValuePair<InputPlayer, Dictionary<string, List<InputModule>>> inputPlayerPrioritizedInputModules in prioritizedInputModules )
			{
				List<InputModule> prioritizedInputModulesList = inputPlayerPrioritizedInputModules.Value[ module.ActionMapName ];
				if ( prioritizedInputModulesList.Count <= 0 )
				{
					prioritizedInputModulesList.Add( module );
					continue;
				}

				if ( prioritizedInputModulesList.First( ).Priority < module.Priority )
				{
					prioritizedInputModulesList.Clear( );
					prioritizedInputModulesList.Add( module );
					continue;
				}

				if ( prioritizedInputModulesList.First( ).Priority == module.Priority ) prioritizedInputModulesList.Add( module );
			}
		}

		public void DeregisterInputModule( InputModule module )
		{
			if ( !activeInputModules.Contains( module ) )
			{
				BetterDebug.Log( $"{module.name}'s InputModule is not registered, can't deregister.", LogSeverity.Warning );
				return;
			}

			activeInputModules.Remove( module );

			string actionMapName = module.ActionMapName;
			foreach ( KeyValuePair<InputPlayer, Dictionary<string, List<InputModule>>> inputPlayerPrioritizedInputModules in prioritizedInputModules )
			{
				List<InputModule> prioritizedInputModulesList = inputPlayerPrioritizedInputModules.Value[ actionMapName ];
				if ( !prioritizedInputModulesList.Contains( module ) ) return;

				prioritizedInputModulesList.Remove( module );

				RefillPriorityList( actionMapName, inputPlayerPrioritizedInputModules.Key );
			}
		}

		public bool IsInputModuleRegistered( InputModule inputModule ) => activeInputModules.Contains( inputModule );

		private void RefillPriorityList( string actionMapName, InputPlayer inputPlayer )
		{
			Dictionary<string, List<InputModule>> inputPlayerPrioritizedInputModules = prioritizedInputModules[ inputPlayer ];
			if ( inputPlayerPrioritizedInputModules[ actionMapName ].Count > 0 ) return;

			int highestPriority = 0;
			List<InputModule> highestPriorityInputModules = new( );
			List<InputModule> noPriorityInputModules = new( );
			foreach ( InputModule activeInputModule in activeInputModules )
			{
				if ( activeInputModule.ActionMapName != actionMapName ) continue;

				if ( activeInputModule.ActiveContexts.Count > 0 && !activeInputModule.ActiveContexts.Contains( currentInputContext ) )
				{
					BetterDebug.Log( "An active InputModule does not have the active InputContext in it's context list, this should not happen.", LogSeverity.Warning );
					continue;
				}

				;

				if ( activeInputModule.Priority == -1 ) noPriorityInputModules.Add( activeInputModule );

				if ( activeInputModule.Priority > highestPriority )
				{
					highestPriorityInputModules.Clear( );
					highestPriority = activeInputModule.Priority;
				}

				if ( activeInputModule.Priority == highestPriority ) highestPriorityInputModules.Add( activeInputModule );
			}

			var mergedList = highestPriorityInputModules.Concat( noPriorityInputModules ).ToList( );
			inputPlayerPrioritizedInputModules[ actionMapName ] = mergedList;
		}

		private void RefillAllPriorityLists( string actionMapName )
		{
			foreach ( InputPlayer refillInputPlayer in inputPlayers ) { RefillPriorityList( actionMapName, refillInputPlayer ); }
		}

		#endregion
	}
}
