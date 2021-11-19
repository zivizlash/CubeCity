using System;
using System.Collections.Generic;

namespace CubeCity.Systems
{
    public interface IGameSystem
    {
    }

    public interface IGameDrawSystem : IGameSystem
    {
        void Draw(TimeSpan elapsed);
    }

    public interface IGameUpdateSystem : IGameSystem
    {
        void Update(TimeSpan elapsed);
    }

    public class GameSystemManager
    {
        private readonly List<IGameDrawSystem> _drawSystems;
        private readonly List<IGameUpdateSystem> _updateSystems;

        public GameSystemManager()
        {
            _drawSystems = new List<IGameDrawSystem>();
            _updateSystems = new List<IGameUpdateSystem>();
        }

        public void Add(IGameSystem gameSystem)
        {
            if (gameSystem is IGameDrawSystem drawSystem)
                _drawSystems.Add(drawSystem);

            if (gameSystem is IGameUpdateSystem updateSystem)
                _updateSystems.Add(updateSystem);
        }

        public void Draw(TimeSpan elapsed)
        {
            foreach (var system in _drawSystems)
            {
                system.Draw(elapsed);
            }
        }

        public void Update(TimeSpan elapsed)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(elapsed);
            }
        }
    }
}
