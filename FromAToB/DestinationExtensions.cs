using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace FromAToB
{
    public static class DestinationExtensions
    {
        public static IDestination ToConsole([DisallowNull] this ISource @source, Func<byte[], string> message) =>
            new Destination().ToConsole(@source, message);

        public static IDestination ToConsole([DisallowNull] this IDestination @destination,
            Func<byte[], string> message) =>
            new Destination().ToConsole(@destination, message);

        public static IDestination ToStream(this ISource @source, Stream stream) =>
            new Destination().ToStream(@source, stream);

        public static IDestination ToStream(this IDestination @destination, Stream stream) =>
            new Destination().ToStream(@destination, stream);
    }
}