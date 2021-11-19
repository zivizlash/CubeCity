using Microsoft.Xna.Framework.Input;

namespace CubeCity.Input
{
    public class GamepadInputManager
    {
        private GamePadState _gamepadState;
        private GamePadState _previousState;

        public GamePadState State => _gamepadState;

        public GamepadInputManager()
        {
            _gamepadState = default;
            _previousState = default;
        }

        public void UpdateGamepadState(GamePadState state)
        {
            _previousState = _gamepadState;
            _gamepadState = state;
        }

        public bool IsButtonDown(Buttons button)
        {
            return _gamepadState.IsButtonDown(button);
        }

        public bool IsButtonUp(Buttons button)
        {
            return _gamepadState.IsButtonUp(button);
        }

        public bool IsButtonReleased(Buttons button)
        {
            return _previousState.IsButtonDown(button) && _gamepadState.IsButtonUp(button);
        }

        public bool IsButtonPressed(Buttons button)
        {
            return _previousState.IsButtonUp(button) && _gamepadState.IsButtonDown(button);
        }
    }
}
