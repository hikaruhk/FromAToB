using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FromAToB;
using NUnit.Framework;

namespace FromAToBTests
{
    public class FlakyMemoryStream : MemoryStream
    {
        public FlakyMemoryStream()
        {
        }

        public FlakyMemoryStream(byte[] bytes) : base(bytes)
        {

        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return count % 5 == 0 
                ? throw new InvalidOperationException("???")
                : base.ReadAsync(buffer, offset, count, cancellationToken);
        }
    }

    [TestFixture]
    public class RecoveryWorkflowTests
    {
        [Test]
        public async Task ShouldLoadFromSourceStreamWithRetry()
        {
            var faker = new Bogus.DataSets.Lorem();
            var messages = new ConcurrentBag<string>();
            var tokenSource = new CancellationTokenSource();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new FlakyMemoryStream(bytes);

            await Source
                .FromStream(memoryStream, (int)memoryStream.Length)
                .ToConsole(data =>
                {
                    var text = Encoding.UTF8.GetString(data);
                    messages.Add($"First:{text}");
                    return text;
                }).ToConsole(data =>
                {
                    var text = Encoding.UTF8.GetString(data);
                    messages.Add($"Second:{text}");
                    return text;
                }).Start(tokenSource.Token);

            messages
                .Should()
                .HaveCount(2)
                .And
                .BeEquivalentTo(
                    $"First:{originalMessage}",
                    $"Second:{originalMessage}");
        }
    }
}