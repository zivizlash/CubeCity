using LearnOpenTK.Components;

namespace LearnOpenTK;

public class World
{
    private readonly List<IUpdatable> _updatables = [];
    private readonly List<IDrawable> _drawables = [];

    public void Update(float elapsed)
    {
        foreach (var updatable in _updatables)
        {
            updatable.Update(elapsed);
        }
    }

    public void Draw()
    {
        foreach (var drawable in _drawables)
        {
            drawable.Draw();
        }
    }

    public World Add(IComponent component)
    {
        if (component is IUpdatable updatable)
        {
            _updatables.Add(updatable);
        }

        if (component is IDrawable drawable)
        {
            _drawables.Add(drawable);
        }

        return this;
    }

    public World Add(IEnumerable<IComponent> components)
    {
        foreach (var component in components)
        {
            Add(component);
        }

        return this;
    }
}
