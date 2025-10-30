using System;

namespace CubeCity.Threading;

public record ProcessorItem(object Value, Func<object, object> Process, Action<ProcessorResult> Add);
