using System;

namespace CubeCity.Services;

public class TimeService : ITime
{
    public TimeSpan ElapsedTime { get; private set; }
    public TimeSpan Time { get; private set; }
    public float Delta => (float)ElapsedTime.TotalSeconds * 60f;

    public void AddTime(TimeSpan elapsedTime)
    {
        Time += elapsedTime;
        ElapsedTime = elapsedTime;
    }

}
