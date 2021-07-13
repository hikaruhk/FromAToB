using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FromAToB
{
    public interface IDestination
    {
        IDestination ToConsole(ISource source, Func<byte[], string> message);
        IDestination ToConsole(IDestination destination, Func<byte[], string> message);
        IDestination ToStream(ISource source, Stream stream);
        ISource Source { get; }
        IReadOnlyList<Func<IConnectableObservable<byte[]>, IDisposable>> SubscriptionDestinations { get; }
        Task Start(CancellationToken cancellationToken);
        Task Start();
    }
}