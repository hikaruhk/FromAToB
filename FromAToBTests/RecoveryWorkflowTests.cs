using System;
using Bogus.DataSets;
using FluentAssertions;
using FromAToB;
using NUnit.Framework;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FromAToBTests
{
    [TestFixture]
    public class RecoveryWorkflowTests
    {
        [Test]
        public async Task ShouldLoadFromSourceStreamWithRetry()
        {
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            using var tokenSource = new CancellationTokenSource();
            await using var memoryStream = new FlakyMemoryStream(
                bytes,
                false, false, false, false);

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