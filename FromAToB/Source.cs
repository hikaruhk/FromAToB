using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace FromAToB
{
    /// <summary>
    ///     A representation of a data source, such as a filesystem, database, or elsewhere.
    /// </summary>
    public class Source : ISource
    {
        /// <summary>
        ///     Creates a data source using a stream that can push data to a destination.
        /// </summary>
        /// <param name="stream">A stream</param>
        /// <param name="bufferSize">Buffer size, defaults to 2048</param>
        /// <param name="offset">The offset to read from every read operation</param>
        /// <returns>A source object</returns>
        public static ISource FromStream(
            Stream stream,
            int bufferSize = 2048,
            int offset = 0)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));

            var source = Observable
                .Defer(() =>
                {
                    var buffer = new byte[bufferSize];
                    var bytesRead = -1;
                    return Observable.While(
                        () => bytesRead != 0,
                        Observable
                            .FromAsync(async () => await stream.ReadAsync(buffer, offset, bufferSize))
                            .Do(d => { bytesRead = d; })
                            .Where(bytesRead => bytesRead != 0)
                            .Select(s => buffer[..s]));
                })
                .ObserveOn(ThreadPoolScheduler.Instance);

            return new FromSource<byte[]>(source);
        }

        /// <summary>
        ///     Creates a data source using a HTTP client that can push data to a destination. Uses GET.
        /// </summary>
        /// <param name="path">A URI</param>
        /// <param name="client">A client</param>
        /// <param name="bufferSize">Buffer size, defaults to 2048</param>
        /// <param name="offset">The offset to read from every read operation</param>
        /// <returns>A source object</returns>
        public static ISource FromHttpGet(
            string path,
            HttpClient client,
            int bufferSize = 2048,
            int offset = 0)
        {
            _ = string.IsNullOrWhiteSpace(path)
                ? throw new ArgumentNullException(nameof(path))
                : path;

            _ = client ?? throw new ArgumentNullException(nameof(client));

            var source = Observable.Defer(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    var stream = await client.GetStreamAsync(path);

                    return FromStream(stream, bufferSize, offset);
                });
            });

            return source.Wait();
        }

        /// <summary>
        ///     Creates a data source using a HTTP client that can push data to a destination. Uses GET.
        /// </summary>
        /// <param name="path">A URI</param>
        /// <param name="bufferSize">Buffer size, defaults to 2048</param>
        /// <param name="offset">The offset to read from every read operation</param>
        /// <returns>A source object</returns>
        public static ISource FromHttpGet(
            string path,
            int bufferSize = 2048,
            int offset = 0)
        {
            _ = string.IsNullOrWhiteSpace(path)
                ? throw new ArgumentNullException(nameof(path))
                : path;

            var source = Observable.Defer(() =>
            {
                return Observable.FromAsync(async () =>
                {
                    using var httpClient = new HttpClient();

                    var stream = await httpClient.GetStreamAsync(path);

                    return FromStream(stream, bufferSize, offset);
                });
            });

            return source.Wait();
        }

        /// <summary>
        ///     Merges one or many sources into one source to be send to a destination.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="first">The first observable</param>
        /// <param name="second">All of the second observables</param>
        /// <returns></returns>
        public static ISource MergeSource<T>(IObservable<T> first, params IObservable<T>[] second)
        {
            _ = first ?? throw new ArgumentNullException(nameof(first));
            _ = second ?? throw new ArgumentNullException(nameof(second));

            if (second.Length == 0) return new FromSource<T>(first);

            var mergedSource = (FromSource<T>)MergeSource(first, second[..^1]);

            return new FromSource<T>(mergedSource.InternalSource.Merge(second[^1]));
        }
    }
}