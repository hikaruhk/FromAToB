using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bogus.DataSets;
using FluentAssertions;
using FromAToB;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace FromAToBTests
{
    [TestFixture]
    public class CommonWorkflowTests
    {
        [Test]
        public async Task ShouldLoadFromSourceStream()
        {
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var tokenSource = new CancellationTokenSource();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new MemoryStream(bytes);

            var source = Source.FromStream(memoryStream, (int) memoryStream.Length);

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
            var faker = new Lorem();
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
                .FromStream(firstMemoryStream, (int) firstMemoryStream.Length)
                .And()
                .FromStream(secondMemoryStream, (int) secondMemoryStream.Length);

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
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new MemoryStream(bytes);

            var source = Source.FromStream(memoryStream, (int) memoryStream.Length);

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
            var faker = new Lorem();
            var messages = new ConcurrentBag<string>();
            var originalFirstMessage = faker.Sentences(1);
            var originalSecondMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalSecondMessage);

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

            await using var memoryStream = new MemoryStream(bytes);

            var source = Source
                .FromStream(memoryStream, (int) memoryStream.Length)
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

        [Test]
        public async Task ShouldLoadFromSourceStreamAndWrite()
        {
            var faker = new Lorem();
            var tokenSource = new CancellationTokenSource();
            var originalMessage = faker.Sentences(1);
            var bytes = Encoding.UTF8.GetBytes(originalMessage);

            await using var memoryStream = new MemoryStream(bytes);
            await using var outgoingStream = new MemoryStream();

            var pipeline = Source
                .FromStream(memoryStream, (int) memoryStream.Length)
                .ToStream(outgoingStream);

            await pipeline.Start(tokenSource.Token);

            outgoingStream.Seek(0, SeekOrigin.Begin);
            using var streamReader = new StreamReader(outgoingStream);

            var results = await streamReader.ReadToEndAsync();

            results
                .Should()
                .Be(originalMessage);
        }
    }
}