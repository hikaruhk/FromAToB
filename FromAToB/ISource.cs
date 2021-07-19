using System;

namespace FromAToB
{
    public interface ISource
    {
        internal IObservable<byte[]> InternalSource { get; set; }
    }
}