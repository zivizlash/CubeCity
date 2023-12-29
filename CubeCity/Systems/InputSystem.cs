using CubeCity.Input;
using CubeCity.Services;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems;

public class InputSystem : IEcsRunSystem
{
    private readonly GamepadInputManager _gamepadManager;
    private readonly KeyboardInputManager _keyboardManager;
    private readonly MouseService _mouseService;

    public InputSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager,
        MouseService mouseService)
    {
        _gamepadManager = gamepadManager;
        _keyboardManager = keyboardManager;
        _mouseService = mouseService;
    }

    public void Run(IEcsSystems systems)
    {
        _gamepadManager.UpdateState(GamePad.GetState(0));
        _keyboardManager.UpdateState(Keyboard.GetState());
        _mouseService.UpdateState(Mouse.GetState());
    }
}
