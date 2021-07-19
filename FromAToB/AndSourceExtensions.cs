using System.IO;
using System.Net.Http;

namespace FromAToB
{
    public static class AndSourceExtensions
    {
        public static ISource FromStream(
            this IAndSource @this,
            Stream stream,
            int bufferSize = 2048,
            int retryCount = 1,
            int offset = 0)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromStream(stream, bufferSize, retryCount, offset);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path,
            int bufferSize = 2048,
            int retryCount = 1,
            int offset = 0)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromHttpGet(path, bufferSize, retryCount, offset);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path,
            HttpClient client,
            int bufferSize = 2048,
            int retryCount = 1,
            int offset = 0)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromHttpGet(path, client, bufferSize, retryCount, offset);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static IAndSource And(this ISource @source) =>
            new AndSource(@source);
    }
}