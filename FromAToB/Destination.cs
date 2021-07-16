using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace FromAToB
{
    public class Destination : IDestination
    {
        public IReadOnlyList<Func<IConnectableObservable<byte[]>, IDisposable>> SubscriptionDestinations { get; private set; }
        public ISource Source { get; private set; }

        public Destination()
        {
            SubscriptionDestinations = new List<Func<IConnectableObservable<byte[]>, IDisposable>>();
        }

        public IDestination ToStream([DisallowNull]ISource source, [DisallowNull]Stream stream)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = stream ?? throw new ArgumentNullException(nameof(stream));

            return new Destination
            {
                Source = source,
                SubscriptionDestinations = new List<Func<IConnectableObservable<byte[]>, IDisposable>>
                {
                    connectable =>
                    {
                        return connectable.Subscribe(
                            async sub => await stream.WriteAsync(sub));
                    }
                }
            };
        }
        public IDestination ToStream([DisallowNull] IDestination destination, [DisallowNull] Stream stream)
        {
            _ = destination ?? throw new ArgumentNullException(nameof(destination));
            _ = stream ?? throw new ArgumentNullException(nameof(stream));

            return new Destination
            {
                Source = destination.Source,
                SubscriptionDestinations = new List<Func<IConnectableObservable<byte[]>, IDisposable>>(
                    destination.SubscriptionDestinations)
                {
                    connectable =>
                    {
                        return connectable.Subscribe(
                            async sub => await stream.WriteAsync(sub));
                    }
                }
            };
        }
        public IDestination ToConsole([DisallowNull] ISource source, [DisallowNull] Func<byte[], string> message)
        {
            _ = source ?? throw new ArgumentNullException(nameof(source));
            _ = message ?? throw new ArgumentNullException(nameof(message));

            return new Destination
            {
                Source = source,
                SubscriptionDestinations = new List<Func<IConnectableObservable<byte[]>, IDisposable>>
                {
                    connectable =>
                    {
                        return connectable.Subscribe(
                            sub => Console.WriteLine(message(sub)));
                    }
                }
            };
        }
        public IDestination ToConsole([DisallowNull] IDestination destination, [DisallowNull] Func<byte[], string> message)
        {
            _ = destination ?? throw new ArgumentNullException(nameof(destination));
            _ = message ?? throw new ArgumentNullException(nameof(message));

            return new Destination
            {
                Source = destination.Source,
                SubscriptionDestinations = new List<Func<IConnectableObservable<byte[]>, IDisposable>>(
                    destination.SubscriptionDestinations)
                {
                    connectable =>
                    {
                        return connectable.Subscribe(
                            sub => Console.WriteLine(message(sub)));
                    }
                }
            };
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            var subs = new List<IDisposable>();
            IDisposable disposable = default;
            try
            {
                var source = Source as FromSource<byte[]>;
                var isComplete = false;
                var publisher = source
                    .InternalSource
                    .Finally(() => isComplete = true)
                    .Publish();

                subs.AddRange(SubscriptionDestinations.Select(sub => sub(publisher)));

                disposable = publisher.Connect();

                while (!cancellationToken.IsCancellationRequested && !isComplete)
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
            catch (TaskCanceledException x)
            {
                System.Diagnostics.Debug.WriteLine($"Task cancelled with message: {x.Message}");
            }
            finally
            {
                disposable?.Dispose();
                foreach (var sub in subs)
                {
                    using var disposedSub = sub;
                }
            }
        }

        public async Task Start()
        {
            var subs = new List<IDisposable>();

            try
            {
                var source = Source as FromSource<byte[]>;
                var publisher = source.InternalSource.Publish();
                subs.AddRange(SubscriptionDestinations.Select(sub => sub(publisher)));

                _ = await publisher;
            }
            finally
            {
                foreach (var sub in subs)
                {
                    using var disposedSub = sub;
                }
            }
        }
    }
}