using System;

namespace CubeCity.Services;

public interface ITime
{
    TimeSpan ElapsedTime { get; }
    TimeSpan Time { get; }
    float Delta { get; }
}
