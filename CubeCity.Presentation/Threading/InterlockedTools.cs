using System.Threading;

namespace CubeCity.Threading;

public static class InterlockedTools
{
    public static bool CompareAndSwap<T>(ref T location1, T value, T comparand) where T : class
    {
        return Interlocked.CompareExchange(ref location1, value, comparand) == comparand;
    }
}
