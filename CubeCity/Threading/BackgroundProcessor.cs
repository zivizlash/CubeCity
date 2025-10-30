using System;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace CubeCity.Threading;

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
