namespace LearnOpenTK.Components;

public interface IComponent
{
}

public interface IDrawable : IComponent
{
    void Draw();
}

public interface IUpdatable : IComponent
{
    void Update(float elapsed);
}
