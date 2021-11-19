using CubeCity.Models;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CubeCity.Systems
{
    public class InputSystem : IGameUpdateSystem
    {
        private readonly GameServices _gameServices;

        public InputSystem(GameServices gameServices)
        {
            _gameServices = gameServices;
        }

        public void Update(TimeSpan elapsed)
        {
            _gameServices.GamepadManager.UpdateGamepadState(GamePad.GetState(PlayerIndex.One));   
            _gameServices.KeyboardManager.UpdateKeyboardState(Keyboard.GetState());
        }
    }
}
