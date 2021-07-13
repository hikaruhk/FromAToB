using FluentAssertions;
using NUnit.Framework;
using FromAToB;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;

namespace FromAToBTests
{
    [TestFixture]
    public class CommonWorkflowTests
    {
        [Test]
        public async Task ShouldLoadFromSourceStream()
        {
            var faker = new Bogus.DataSets.Lorem();
            var messages = new ConcurrentBag<string>();
            var tokenSource = new CancellationTokenSource();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new MemoryStream(bytes);

            var source = Source.FromStream(memoryStream, (int) memoryStream.Length, 0);

            var firstDestination = source.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"First:{text}");
                return text;
            });
            var secondDestination = firstDestination.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"Second:{text}");
                return text;
            });

            await secondDestination.Start(tokenSource.Token);

            messages
                .Should()
                .HaveCount(2)
                .And
                .BeEquivalentTo(
                    $"First:{originalMessage}",
                    $"Second:{originalMessage}");
        }

        [Test]
        public async Task ShouldLoadFromMultipleSourceStream()
        {
            var faker = new Bogus.DataSets.Lorem();
            var messages = new ConcurrentBag<string>();
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var originalFirstMessage = faker.Sentences(1);
            var originalSecondMessage = faker.Sentences(1);
            var firstBytes = Encoding.UTF8.GetBytes(originalFirstMessage);
            var secondBytes = Encoding.UTF8.GetBytes(originalSecondMessage);

            await using var firstMemoryStream = new MemoryStream(firstBytes);
            await using var secondMemoryStream = new MemoryStream(secondBytes);

            var source = Source
                .FromStream(firstMemoryStream, (int) firstMemoryStream.Length, 0)
                .And()
                .FromStream(secondMemoryStream, (int) secondMemoryStream.Length, 0);

            var firstDestination = source.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"First:{text}");
                return text;
            });
            var secondDestination = firstDestination.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"Second:{text}");
                return text;
            });

            await secondDestination.Start(tokenSource.Token);

            messages
                .Should()
                .HaveCount(4)
                .And
                .BeEquivalentTo(
                    $"First:{originalFirstMessage}",
                    $"Second:{originalFirstMessage}",
                    $"First:{originalSecondMessage}",
                    $"Second:{originalSecondMessage}");
        }

        [Test]
        public async Task ShouldLoadFromSourceWithoutToken()
        {
            var faker = new Bogus.DataSets.Lorem();
            var messages = new ConcurrentBag<string>();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new MemoryStream(bytes);

            var source = Source.FromStream(memoryStream, (int) memoryStream.Length, 0);

            var firstDestination = source.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"First:{text}");
                return text;
            });
            var secondDestination = firstDestination.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"Second:{text}");
                return text;
            });

            await secondDestination.Start();

            messages
                .Should()
                .HaveCount(2)
                .And
                .BeEquivalentTo(
                    $"First:{originalMessage}", $"Second:{originalMessage}");
        }

        [Test]
        public async Task ShouldLoadFromMixedSource()
        {
            var faker = new Bogus.DataSets.Lorem();
            var messages = new ConcurrentBag<string>();
            var originalFirstMessage = faker.Sentences(1);
            var originalSecondMessage = faker.Sentences(1);
            var firstBytes = Encoding.UTF8.GetBytes(originalFirstMessage);
            var secondBytes = Encoding.UTF8.GetBytes(originalSecondMessage);

            var httpHandler = new Mock<HttpMessageHandler>();

            httpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Returns(async () =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(originalFirstMessage)
                    };
                });

            using var client = new HttpClient(httpHandler.Object);

            await using var memoryStream = new MemoryStream(secondBytes);

            var source = Source
                .FromStream(memoryStream, (int) memoryStream.Length, 0)
                .And()
                .FromHttpGet(
                    "https://rickandmortyapi.com/api/character/",
                    client);

            var firstDestination = source.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"First:{text}");
                return text;
            });
            var secondDestination = firstDestination.ToConsole(data =>
            {
                var text = Encoding.UTF8.GetString(data);
                messages.Add($"Second:{text}");
                return text;
            });

            await secondDestination.Start();

            messages
                .Should()
                .HaveCount(4)
                .And
                .BeEquivalentTo(
                    $"First:{originalFirstMessage}",
                    $"Second:{originalFirstMessage}",
                    $"First:{originalSecondMessage}",
                    $"Second:{originalSecondMessage}");
        }
    }
}