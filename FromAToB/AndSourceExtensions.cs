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
            int offset = 0)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromStream(stream, bufferSize, offset);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromHttpGet(path);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path,
            HttpClient client)
        {
            var firstSource = @this.GetParent;
            var secondSource = Source.FromHttpGet(path, client);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static IAndSource And(this ISource @source) =>
            new AndSource(@source);
    }
}