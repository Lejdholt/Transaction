using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Akka.Actor;
using Confluent.Kafka;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Trace;
using Transaction.Service.Account;

namespace Transaction.Service
{
    public class KafkaActor : ReceiveActor
    {
        private static readonly ActivitySource ActivitySource = new(nameof(KafkaActor));
        private static readonly TextMapPropagator TraceContextPropagator = new TraceContextPropagator();
        private readonly IProducer<string, string> _producer;
        private readonly string _topic = "test";

        public KafkaActor()
        {
            _producer = new ProducerBuilder<string, string>(new ProducerConfig
                {
                    BootstrapServers = "broker:9092",
                    ClientId = Dns.GetHostName(),
                    QueueBufferingMaxKbytes = 25600,
                    QueueBufferingMaxMessages = 1000
                })
                .Build();


            ReceiveAsync<ObservableEnvelope>(async e =>
                {
                
                    var headers = new Headers();
                    var messageType = e.Body.GetType().Name;

                    headers.Add("type", Encoding.UTF8.GetBytes(messageType));

                    using var activity = ActivitySource.StartActivity("test send", ActivityKind.Internal, e.ParentContext);

                    if (activity != default)
                    {
                        activity.AddTag("messaging.system", "kafka")
                            .AddTag("messaging.destination_kind", "topic")
                            .AddTag("messaging.destination", _topic)
                            .AddTag("messaging.type", messageType);

                        TraceContextPropagator.Inject(new PropagationContext(activity.Context, Baggage.Current), headers,
                            (kafkaHeaders, key, value) => { kafkaHeaders.Add(new Header(key, Encoding.UTF8.GetBytes(value))); });
                    }

                
                    try
                    {
                        using (KafkaMetrics.RecordTopicDuration(_topic, messageType))
                        {
                            var value = JsonSerializer.Serialize(e.Body);

                            var result =     await _producer.ProduceAsync(_topic, new Message<string, string>
                            {
                                Headers = headers,
                                Key = FindKey(e.Body),
                                Value = value
                            });
                        }

                       

                        KafkaMetrics.RecordTopicProducedMessage(_topic, messageType);
                    }
                    catch (Exception ex)
                    {
                        KafkaMetrics.RecordTopicProducingError(_topic, messageType);
                        activity.RecordException(ex);

                        throw;
                    }
                }
            );
        }

        private string FindKey(object eBody)
        {
            return eBody switch
            {
                ITransactionEvent e => e.Id.Id.ToString(),
                IAccountEvent e => e.Id.Id,
                _ => throw new NotSupportedException(eBody.GetType().FullName)
            };
        }
    }
}