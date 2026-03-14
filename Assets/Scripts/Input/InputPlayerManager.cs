using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace DigDig2.Input
{
	public abstract class InputPlayerManager : MonoBehaviour
	{
		public UnityEvent<InputPlayer, int> inputPlayerAdded;
		public UnityEvent<InputPlayer, int> inputPlayerReconnected;
		public UnityEvent<InputPlayer, int> inputPlayerDisconnected;
		
		protected bool hasGeneratedList = false;
		
		public abstract List<InputPlayer> GenerateList( );
		public abstract List<InputPlayer> AddDevice( List<InputPlayer> inputPlayers, InputDevice device );
		public abstract List<InputPlayer> RemoveDevice( List<InputPlayer> inputPlayers, InputDevice device );
	}
}
