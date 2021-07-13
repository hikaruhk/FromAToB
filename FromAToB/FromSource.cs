using System;

namespace FromAToB
{
    internal class FromSource<T> : ISource
    {
        public IObservable<T> InternalSource { get; }

        public FromSource(IObservable<T> source)
        {
            InternalSource = source;
        }
    }
}