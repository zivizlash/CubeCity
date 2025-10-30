using System;
using System.Threading;

namespace CubeCity.Threading;

public class BackgroundManager
{
    private readonly BackgroundProcessor _processor;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public BackgroundManager()
    {
        _processor = new BackgroundProcessor(_cts.Token);
    }

    public IProcessorPipe<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> process)
    {
        return new BackgroundProcessorPipe<TIn, TOut>(_processor, process);
    }

    public void Stop()
    {
        if (!_disposed)
        {
            _disposed = true;
            _cts.Cancel();
        }
    }
}
