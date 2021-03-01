using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using FluentAssertions;

namespace Transaction.Service.Tests
{
    public class IntegrationTestBase
    {
        protected HttpClient? Client;
        protected IConsumer<string, string> Consumer;
        protected CancellationTokenSource Source;

        public IntegrationTestBase()
        {
            Source = new CancellationTokenSource();

            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:29092",
                GroupId = Guid.NewGuid().ToString(),
                EnableAutoCommit = true,
                EnableAutoOffsetStore = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnablePartitionEof = true
            };

            Consumer = new ConsumerBuilder<string, string>(config)
                .Build();
            Consumer.Subscribe("test");

            Client = new HttpClient
            {
                BaseAddress = new Uri("https://localhost/")
            };
        }

        protected void ExpectMessageOnKafka<T>(object expectation)
        {
            Exception failure = null;
            Source.CancelAfter(5000);
            while (!Source.IsCancellationRequested)
            {
                var consumeResult = Consumer.Consume(Source.Token);

                if (consumeResult.IsPartitionEOF)
                {
                    continue;
                }

                var headers = consumeResult.Message.Headers.ToDictionary(h => h.Key, h => Encoding.UTF8.GetString(h.GetValueBytes()));

                if (headers["type"] == typeof(T).Name)
                {
                    var message = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);

                    try
                    {
                        message.Should().BeEquivalentTo(expectation);
                        Consumer.StoreOffset(consumeResult);
                        return;
                    }
                    catch (Exception a)
                    {
                        failure = a;
                        continue;
                    }
                }


                Consumer.StoreOffset(consumeResult);
            }

            if (failure is not null)
            {
                throw failure;
            }
        }

        protected async Task<HttpResponseMessage> Post(string uri, object body)
        {
            var content = JsonSerializer.Serialize(body);

            return await Client.PostAsync(new Uri(uri, UriKind.Relative), new StringContent(content, Encoding.UTF8, "application/json"));
        }
    }
}