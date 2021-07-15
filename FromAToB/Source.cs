using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace FromAToB
{
    public class Source : ISource
    {
        public static ISource FromStream(Stream stream, int bufferSize = 2048, int offset = 0)
        {
            var source = Observable
                .Defer(() =>
                {
                    var buffer = new byte[bufferSize];
                    var bytesRead = -1;
                    return Observable.While(
                        () => bytesRead != 0,
                        Observable
                            .FromAsync(async () => await stream.ReadAsync(buffer, offset, bufferSize))
                            .Retry(5)
                            .Do(d => { bytesRead = d; })
                            .Where(bytesRead => bytesRead != 0)
                            .Select(s => buffer[..s]));
                })
                .ObserveOn(ThreadPoolScheduler.Instance);

            return new FromSource<byte[]>(source);
        }

        public static ISource FromHttpGet(string path, HttpClient client)
        {
            var source = Observable.Defer(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    var stream = await client.GetStreamAsync(path);

                    return FromStream(stream);
                });
            });

            return source.Wait();
        }

        public static ISource FromHttpGet(string path)
        {
            var source = Observable.Defer(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    using var httpClient = new HttpClient();

                    var stream = await httpClient.GetStreamAsync(path);

                    return FromStream(stream);
                });
            });

            return source.Wait();
        }

        public static ISource MergeSource<T>(IObservable<T> first, IObservable<T> second) =>
            new FromSource<T>(first.Merge(second));
    }
}