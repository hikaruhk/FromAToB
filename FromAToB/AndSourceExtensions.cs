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
            var firstSource = (FromSource<byte[]>) @this.GetParent;
            var secondSource = (FromSource<byte[]>) Source.FromStream(stream, bufferSize, offset);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path)
        {
            var firstSource = (FromSource<byte[]>)@this.GetParent;
            var secondSource = (FromSource<byte[]>)Source.FromHttpGet(path);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }

        public static ISource FromHttpGet(
            this IAndSource @this,
            string path,
            HttpClient client)
        {
            var firstSource = (FromSource<byte[]>)@this.GetParent;
            var secondSource = (FromSource<byte[]>)Source.FromHttpGet(path, client);

            return Source.MergeSource(firstSource.InternalSource, secondSource.InternalSource);
        }
    }
}