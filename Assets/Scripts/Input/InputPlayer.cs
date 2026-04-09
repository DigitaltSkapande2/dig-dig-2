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
            if (active) InputManager.Instance.OnActionTriggered(this, context);
            else BetterDebug.Log($"ME: {name}  triggering action {context.action.name} but i am INACTIVE");
        }

		public override string ToString( ) => $"InputPlayer ({name})";
	}
}
