using System;
using System.Collections.Generic;
using DigDig2.Debugging;
using UnityEngine.InputSystem;

namespace DigDig2.Input
{
    [Serializable]
	public class InputPlayer
	{
		public readonly List<InputDevice> connectedDevices = new( );
		public string name;
		public bool active = true;
        public readonly List<InputActionMap> actionMaps = new( );

        public void OnActionTriggered(InputAction.CallbackContext context)
        {
            InputManager.Instance.OnActionTriggered(this, context);
        }

		public override string ToString( ) => $"InputPlayer ({name})";
	}
}
