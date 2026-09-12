using Game.Input.Interfaces;
using UnityEngine.InputSystem;
using static Game.UI.Constants.HudConstants;

namespace Game.Input.Managers
{
    public class DebugToggleInput : IDebugInput
    {
        public bool ConsumeToggle()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return false;
            return keyboard[ToggleOverlayKey].wasPressedThisFrame;
        }
    }
}
