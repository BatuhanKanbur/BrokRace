using Game.Input.Interfaces;
using UnityEngine.InputSystem;
using static Game.UI.Constants.HudConstants;

namespace Game.Input.Managers
{
    public class MenuInput : IMenuInput
    {
        public bool ConsumeOverlayToggle() => WasPressed(ToggleOverlayKey);

        public bool ConsumeRestart() => WasPressed(RestartKey);

        private static bool WasPressed(Key key)
        {
            var keyboard = Keyboard.current;
            return keyboard != null && keyboard[key].wasPressedThisFrame;
        }
    }
}
