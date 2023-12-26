using System;

namespace CubeCity.Services;

public interface ITime
{
    TimeSpan ElapsedTime { get; }
    TimeSpan Time { get; }
}

public class TimeService : ITime
{
    public TimeSpan ElapsedTime { get; private set; }
    public TimeSpan Time { get; private set; }

    public void AddTime(TimeSpan elapsedTime)
    {
        Time += elapsedTime;
        ElapsedTime = elapsedTime;
    }
}
