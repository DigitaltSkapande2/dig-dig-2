using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

namespace DigDig2.Input
{
	public class InputModule : MonoBehaviour
	{
		public const string INPUT_MESSAGE_PREFIX = "OnInput";
		public const string INPUT_MESSAGE_SUFFIX = "";

		[Tooltip( "The priority that this InputModule has against all other InputModules. Use -1 to get inputs without taking priority into account." )]
		[SerializeField] private int priority;

		[Tooltip( "The InputPlayer that this InputModule will take inputs from. Use -1 to get inputs from all InputPlayers." )]
		[SerializeField] private int allowedInputPlayerIndex = -1;

		[Tooltip( "The name of the InputActionMap that the InputModule will use." )]
		[SerializeField] private string actionMapName;

		[Tooltip( "The contexts where this module will be used in. SHOULD NOT BE EDITED IN PLAY MODE, USE ActiveContexts IN SCRIPTS." )]
		[SerializeField] private List<InputContext> activeContexts = new( );

		private bool hasStarted;

		public int Priority
		{
			get => priority;
		}

		public int AllowedInputPlayerIndex
		{
			get => allowedInputPlayerIndex;
		}

		public string ActionMapName
		{
			get => actionMapName;
		}

		public List<InputContext> ActiveContexts
		{
			get => activeContexts;
			set
			{
				activeContexts = value;
				OnContextsChanged( );
			}
		}

		public bool IsRegistered
		{
			get => InputManager.Instance && InputManager.Instance.IsInputModuleRegistered( this );
		}

		public bool CanBeRegistered
		{
			get => activeContexts.Count <= 0 || activeContexts.Contains( InputManager.Instance.CurrentInputContext );
		}

		private void Start( )
		{
			RefreshRegistration( );
			hasStarted = true;
		}

		private void OnEnable( )
		{
			if ( hasStarted ) RefreshRegistration( );
		}

		private void OnDisable( ) { Deregister( ); }

		#region Input

		// ReSharper disable once MemberCanBeMadeStatic.Global
		public void SendInput( InputAction.CallbackContext context, InputPlayer inputPlayer, int inputPlayerIndex )
		{
			InputInfo inputInfo = new( )
			{
				context = context,
				inputPlayer = inputPlayer,
				inputPlayerIndex = inputPlayerIndex
			};

			SendMessage( $"{INPUT_MESSAGE_PREFIX}{context.action.actionMap.name}{context.action.name}{INPUT_MESSAGE_SUFFIX}", inputInfo, SendMessageOptions.DontRequireReceiver );
		}

		#endregion

		#region Context Updates

		private void OnContextsChanged( ) { RefreshRegistration( ); }

		#endregion

		#region Registration

		private void Register( )
		{
			if ( InputManager.Instance && CanBeRegistered ) InputManager.Instance.RegisterInputModule( this );
		}

		private void Deregister( )
		{
			if ( InputManager.Instance ) InputManager.Instance.DeregisterInputModule( this );
		}

		public void RefreshRegistration( )
		{
			if ( IsRegistered )
			{
				if ( !CanBeRegistered ) Deregister( );
			}
			else
			{
				if ( CanBeRegistered ) Register( );
			}
		}

		#endregion
	}

	public struct InputInfo
	{
		public InputAction.CallbackContext context;
		public InputPlayer inputPlayer;
		public int inputPlayerIndex;
	}
}
