using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace CubeCity.Threading;

public interface IProcessorPipe<TIn, TOut>
{
    void Enqueue(TIn input);
    bool TryPoll([NotNullWhen(true)] out TOut? value);
}

public class BackgroundProcessorPipe<TIn, TOut>(BackgroundProcessor processor, Func<TIn, TOut> process)
    : IProcessorPipe<TIn, TOut>
{
    private readonly ConcurrentQueue<ProcessorResult> _results = new();
    private readonly Func<object, object> _processDelegate = input => process.Invoke((TIn)input)!;

    public void Enqueue(TIn input)
    {
        processor.Enqueue(input!, _processDelegate, AddToResults);
    }

    public bool TryPoll([NotNullWhen(true)] out TOut? value)
    {
        if (_results.TryDequeue(out var result))
        {
            if (result.Exception is not null)
            {
                throw new AggregateException("Exception while processing request", result.Exception);
            }

            value = (TOut)result.Result!;
            return true;
        }

        value = default;
        return false;
    }

    private void AddToResults(ProcessorResult result)
    {
        _results.Enqueue(result);
    }
}
