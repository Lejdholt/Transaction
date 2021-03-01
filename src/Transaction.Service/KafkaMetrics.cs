using System;
using System.Collections.Generic;
using Prometheus;

namespace Transaction.Service
{
    public static class KafkaMetrics
    {
        private const string MessageTypeLabel = @"message_type";
        private const string TopicLabel = @"topic";

        private static readonly string[] LabelNames = {TopicLabel, MessageTypeLabel};

        private static readonly Counter ProduceMessageErrorCounter = Metrics.CreateCounter("message_produce_error_total", "Number of messages that failed processing", LabelNames);
        private static readonly Counter ProducedMessagesCounter = Metrics.CreateCounter("messages_produced_total", "Number of messages processed", LabelNames);

        private static readonly Histogram ProducedMessageDurationHistogram = Metrics.CreateHistogram("messages_produced_duration_seconds", "Messages processing duration",
            new HistogramConfiguration
            {
                Buckets = CreateBuckets(),
                LabelNames = LabelNames
            });


        public static void RecordTopicProducingError(string topic, string messageType)
        {
            ProduceMessageErrorCounter.WithLabels(topic, messageType).Inc();
        }

        public static void RecordTopicProducedMessage(string topic, string messageType)
        {
            ProducedMessagesCounter.WithLabels(topic, messageType).Inc();
        }

        public static IDisposable RecordTopicDuration(string topic, string messageType) => ProducedMessageDurationHistogram.WithLabels(topic, messageType).NewTimer();


        private static double[] CreateBuckets()
        {
            var buckets = new List<double> {0.001, 0.002, 0.003, 0.004, 0.005, 0.006, 0.008};
            buckets.AddRange(Histogram.ExponentialBuckets(0.01, 2, 10));

            return buckets.ToArray();
        }
    }
}