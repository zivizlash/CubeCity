using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace CubeCity.Threading;

public class BackgroundProcessorPipe<TIn, TOut>(BackgroundProcessor processor, Func<TIn, TOut> process)
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

public record ProcessorItem(object Value, Func<object, object> Process, Action<ProcessorResult> Add);

public record ProcessorResult(object? Result, Exception? Exception);

public class BackgroundProcessor
{
    private readonly ActionBlock<ProcessorItem> _actionBlock;

    public BackgroundProcessor(CancellationToken cancellationToken)
    {
        _actionBlock = new ActionBlock<ProcessorItem>(ProcessInternal, new ExecutionDataflowBlockOptions
        {
            SingleProducerConstrained = true,
            MaxDegreeOfParallelism = Environment.ProcessorCount / 2,
            EnsureOrdered = false,
            BoundedCapacity = 2048,
            CancellationToken = cancellationToken
        });
    }

    public void Enqueue(object value, Func<object, object> process, Action<ProcessorResult> add)
    {
        var processorItem = new ProcessorItem(value, process, add);

        if (!_actionBlock.Post(processorItem))
        {
            throw new InvalidOperationException();
        }
    }

    private void ProcessInternal(ProcessorItem item)
    {
        try
        {
            var processed = item.Process.Invoke(item.Value);
            var result = new ProcessorResult(processed, null);
            item.Add.Invoke(result);
        }
        catch (Exception ex)
        {
            var result = new ProcessorResult(null, ex);
            item.Add.Invoke(result);
        }
    }
}

public class BackgroundManager
{
    private readonly BackgroundProcessor _processor;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    public BackgroundManager()
    {
        _processor = new BackgroundProcessor(_cts.Token);
    }

    public BackgroundProcessorPipe<TIn, TOut> Create<TIn, TOut>(Func<TIn, TOut> process)
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
