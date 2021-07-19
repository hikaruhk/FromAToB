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
        [TestCase(5, new[] { false, false, false, false, false }, Description = "Success, no failures")]
        [TestCase(5, new[] { true, false, false, false, false }, Description = "Success, one failures")]
        [TestCase(5, new[] { true, true, true, true, false}, Description = "Success, five failures")]
        public async Task ShouldLoadFromSourceStreamWithRetry(int retryCount, bool[] errorSet)
        {
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            using var tokenSource = new CancellationTokenSource();
            await using var memoryStream = new FlakyMemoryStream(bytes, errorSet);

            await Source
                .FromStream(memoryStream, (int)memoryStream.Length, retryCount)
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

        [TestCase(new[] { true, true, true, true, true, true }, Description = "Failure, six failures")]
        public async Task ShouldNotLoadFromSourceStreamWithRetry(bool[] errorSet)
        {
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            using var tokenSource = new CancellationTokenSource();
            await using var memoryStream = new FlakyMemoryStream(bytes, errorSet);

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
                .HaveCount(0);
        }
    }
}