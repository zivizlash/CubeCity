namespace LearnOpenTK.Providers;

public interface ITimeProvider
{
    float Elapsed { get; }
    float Total { get; }
}
