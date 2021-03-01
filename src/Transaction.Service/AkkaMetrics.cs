using System;
using System.Collections.Generic;
using Akka.Actor;
using Prometheus;

namespace Transaction.Service
{
    public static class AkkaMetrics
    {
        private const string MessageTypeLabel = @"message_type";
        private const string PathLabel = @"path";
        private const string NameLabel = @"name";

        private static readonly string[] LabelNames = {PathLabel, NameLabel, MessageTypeLabel};

        private static readonly Counter ProcessMessageErrorCounter = Metrics.CreateCounter("message_process_error_total", "Number of messages that failed processing", LabelNames);
        private static readonly Counter ProcessedMessagesCounter = Metrics.CreateCounter("messages_processed_total", "Number of messages processed", LabelNames);

        private static readonly Histogram ProcessedMessageDurationHistogram = Metrics.CreateHistogram("messages_processed_duration_seconds", "Messages processing duration",
            new HistogramConfiguration
            {
                Buckets = CreateBuckets(),
                LabelNames = LabelNames
            });


        public static void RecordProcessingError<T>(ActorPath path)
        {
            ProcessMessageErrorCounter.WithLabels(path.ToString(), path.Name, typeof(T).Name).Inc();
        }

        public static void RecordProcessedMessage<T>(ActorPath path)
        {
            ProcessedMessagesCounter.WithLabels(path.ToString(), path.Name, typeof(T).Name).Inc();
        }

        public static IDisposable RecordDuration<T>(ActorPath path) => ProcessedMessageDurationHistogram.WithLabels(path.ToString(), path.Name, typeof(T).Name).NewTimer();

        private static double[] CreateBuckets()
        {
            var buckets = new List<double> {0.001, 0.002, 0.003, 0.004, 0.005, 0.006, 0.008};
            buckets.AddRange(Histogram.ExponentialBuckets(0.01, 2, 10));

            return buckets.ToArray();
        }
    }
}