using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Akka.Actor;
using OpenTelemetry.Trace;

namespace Transaction.Service
{
    public class ObservableReceiveActor : ReceiveActor
    {
        public ObservableReceiveActor()
        {

        }

        private static readonly ActivitySource ActivitySource = new(nameof(ObservableReceiveActor));


        protected new void ReceiveAsync<T>(Func<T, Task> handler, Predicate<T>? shouldHandle = null) where T : notnull
        {
            base.ReceiveAsync<ObservableEnvelope<T>>(async envelope =>
            {
                using var activity = ActivitySource.StartActivity($"Receive {envelope.Body.GetType().Name}", ActivityKind.Internal, envelope.ParentContext);

                if (activity != default)
                {
                    activity.AddTag("akka.path", this.Self.Path);
                    activity.AddTag("akka.name", this.Self.Path.Name);

                }

                try
                {
                    using (AkkaMetrics.RecordDuration<T>(Self.Path))
                    {
                        await handler(envelope.Body);
                    }

                    AkkaMetrics.RecordProcessedMessage<T>(Self.Path);
                }
                catch (Exception ex)
                {
                    AkkaMetrics.RecordProcessingError<T>(Self.Path);
                    activity.RecordException(ex);

                    throw;
                }

            }, envelope => shouldHandle?.Invoke(envelope.Body) ?? true);
        } 
        
        
        protected new void Receive<T>(Action<T> handler, Predicate<T>? shouldHandle = null) where T : notnull
        {
            base.Receive<ObservableEnvelope<T>>(envelope =>
            {
                using var activity = ActivitySource.StartActivity($"Receive {envelope.Body.GetType().Name}", ActivityKind.Internal, envelope.ParentContext);

                if (activity != default)
                {
                    activity.AddTag("akka.path", this.Self.Path);
                    activity.AddTag("akka.name", this.Self.Path.Name);

                }

                try
                {
                    using (AkkaMetrics.RecordDuration<T>(Self.Path))
                    {
                        handler(envelope.Body);
                    }

                    AkkaMetrics.RecordProcessedMessage<T>(Self.Path);
                }
                catch (Exception ex)
                {
                    AkkaMetrics.RecordProcessingError<T>(Self.Path);
                    activity.RecordException(ex);

                    throw;
                }

            }, envelope => shouldHandle?.Invoke(envelope.Body) ?? true);
        }
    }

    public record ObservableEnvelope<T>:ObservableEnvelope where T:notnull
    {
        public ObservableEnvelope(T body, ActivityContext parentContext) : base(body, parentContext)
        {
        } 
        
        
        public ObservableEnvelope(T body) : base(body, Activity.Current.Context)
        {
        }

        public new T Body => (T)base.Body;
    };
}