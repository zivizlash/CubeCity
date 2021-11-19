using System;

namespace CubeCity.Tools
{
    public readonly struct Pooled<T> : IDisposable
    {
        public T Resource { get; }
        private readonly Action<T> _free;

        public Pooled(T resource, Action<T> free)
        {
            Resource = resource;
            _free = free;
        }

        public void Dispose()
        {
            _free.Invoke(Resource);
        }
    }
}
