namespace LearnOpenTK.Providers;

public class ManualTimeProvider : ITimeProvider
{
    public float Elapsed { get; private set; }
    public float Total { get; private set; }

    public void AddAndUpdateElapsed(float elapsed)
    {
        Total += elapsed;
        Elapsed = elapsed;
    }
}
