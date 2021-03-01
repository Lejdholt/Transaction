using System.Diagnostics;

namespace Transaction.Service
{
    public record ObservableEnvelope(object Body, ActivityContext ParentContext);
}