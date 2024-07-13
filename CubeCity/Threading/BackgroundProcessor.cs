using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks.Dataflow;

namespace CubeCity.Threading;

public interface IBackgroundProcessor<TIn, TOut>
{
    TOut Process(TIn arg);
}

public interface IBackgroundProcessingEntry<TIn, TOut> : IDisposable where TIn : notnull
{
    void Enqueue(TIn item);
    bool TryGetResult([NotNullWhen(true)] out TOut? item);
}

public class BackgroundProcessingExecutor
{
    private readonly ActionBlock<BackgroundProcessingRegistryActionItem> _actionBlock;
    private ImmutableDictionary<Type, IBackgroundProcessingData> _typeToProcessor;

    public BackgroundProcessingExecutor()
    {
        _actionBlock = new ActionBlock<BackgroundProcessingRegistryActionItem>(ProcessInternal,
            new ExecutionDataflowBlockOptions
            {
                SingleProducerConstrained = true,
                MaxDegreeOfParallelism = Environment.ProcessorCount / 2
            });
        _typeToProcessor = ImmutableDictionary.Create<Type, IBackgroundProcessingData>();
    }

    public IBackgroundProcessingEntry<TIn, TOut> Register<TIn, TOut>(Func<TIn, TOut> processor)
        where TIn : notnull
    {
        var item = new BackgroundProcessingData<TIn, TOut>(processor);

        var typeToProcessor = _typeToProcessor;

        while (!InterlockedTools.CompareAndSwap(ref _typeToProcessor, 
            typeToProcessor.Add(typeof(TIn), item), typeToProcessor))
        {
            typeToProcessor = _typeToProcessor;
        }

        return new ProcessingEntryDataAdapter<TIn, TOut>(item, this);
    }

    public void Post<TIn>(TIn argument) where TIn : notnull
    {
        var processingData = _typeToProcessor[typeof(TIn)];
        _actionBlock.Post(new BackgroundProcessingRegistryActionItem(processingData, argument));
    }

    private void ProcessInternal(BackgroundProcessingRegistryActionItem item)
    {
        item.ProcessingData.Process(item.Arg);
    }
}

public class BackgroundProcessingRegistry
{
    private readonly BackgroundProcessingExecutor _executor;

    public BackgroundProcessingRegistry()
    {
        _executor = new BackgroundProcessingExecutor();
    }

    public IBackgroundProcessingEntry<TIn, TOut> Register<TIn, TOut>(Func<TIn, TOut> processor) where TIn : notnull
    {
        return _executor.Register(processor);
    }
}

public interface IBackgroundProcessingData
{
    void Process(object arg);
}

public class BackgroundProcessingData<TIn, TOut> : IBackgroundProcessingData
{
    public ConcurrentQueue<TOut> Processed { get; set; }
    public Func<TIn, TOut> Processor { get; set; }
    public Type InputType { get; }

    public BackgroundProcessingData(Func<TIn, TOut> processor)
    {
        Processed = new ConcurrentQueue<TOut>();
        Processor = processor;
        InputType = typeof(TIn);
    }

    public void Process(object arg)
    {
        if (arg is TIn item)
        {
            var result = Processor.Invoke(item);
            Processed.Enqueue(result);
        }
    }
}

public class ProcessingEntryDataAdapter<TIn, TOut> : IBackgroundProcessingEntry<TIn, TOut> where TIn : notnull
{
    private readonly BackgroundProcessingData<TIn, TOut> _processingData;
    private readonly BackgroundProcessingExecutor _executor;
    private readonly Action _disposeAction;
    private bool _disposed;

    public ProcessingEntryDataAdapter(BackgroundProcessingData<TIn, TOut> processingData, BackgroundProcessingExecutor executor)
    {
        _processingData = processingData;
        _executor = executor;
        _disposeAction = () => { };
    }
    public void Enqueue(TIn item)
    {
        _executor.Post(item);
    }

    public bool TryGetResult([NotNullWhen(true)] out TOut? item)
    {
#pragma warning disable CS8762 // Parameter must have a non-null value when exiting in some condition.
        return _processingData.Processed.TryDequeue(out item);
#pragma warning restore CS8762
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _disposeAction.Invoke();
    }
}

public class BackgroundProcessingRegistryActionItem
{
    public IBackgroundProcessingData ProcessingData { get; }
    public object Arg { get; }

    public BackgroundProcessingRegistryActionItem(IBackgroundProcessingData processingData, object arg)
    {
        ProcessingData = processingData;
        Arg = arg;
    }
}
