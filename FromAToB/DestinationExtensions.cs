using System;
using System.Diagnostics.CodeAnalysis;

namespace FromAToB
{
    public static class DestinationExtensions
    {
        public static IDestination ToConsole([DisallowNull] this ISource @source, Func<byte[], string> message) =>
            new Destination().ToConsole(@source, message);

        public static IDestination ToConsole([DisallowNull] this IDestination @destination,
            Func<byte[], string> message) =>
            new Destination().ToConsole(@destination, message);
    }
}