using Microsoft.Xna.Framework.Input;

namespace CubeCity.Input;

public class KeyboardInputManager
{
    private KeyboardState _keyboardState;
    private KeyboardState _previousState;

    public KeyboardState State => _keyboardState;

    public KeyboardInputManager()
    {
        _keyboardState = default;
        _previousState = default;
    }

    public void UpdateKeyboardState(KeyboardState state)
    {
        _previousState = _keyboardState;
        _keyboardState = state;
    }

    public bool IsKeyDown(Keys key)
    {
        return _keyboardState.IsKeyDown(key);
    }

    public bool IsKeyUp(Keys key)
    {
        return _keyboardState.IsKeyUp(key);
    }

    public bool IsKeyReleased(Keys key)
    {
        return _previousState.IsKeyDown(key) && _keyboardState.IsKeyUp(key);
    }

    public bool IsKeyPressed(Keys key)
    {
        return _previousState.IsKeyUp(key) && _keyboardState.IsKeyDown(key);
    }
}
