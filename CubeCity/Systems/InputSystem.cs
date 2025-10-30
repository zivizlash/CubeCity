using CubeCity.Input;
using CubeCity.Services;
using Leopotam.EcsLite;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems;

public class InputSystem(GamepadInputManager gamepadManager, KeyboardInputManager keyboardManager,
    MouseService mouseService) : IEcsRunSystem
{
    public void Run(IEcsSystems systems)
    {
        gamepadManager.UpdateState(GamePad.GetState(0));
        keyboardManager.UpdateState(Keyboard.GetState());
        mouseService.UpdateState(Mouse.GetState());
    }
}
