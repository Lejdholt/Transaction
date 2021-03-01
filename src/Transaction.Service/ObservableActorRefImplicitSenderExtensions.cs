using System.Diagnostics;
using System.Threading.Tasks;
using Akka.Actor;

namespace Transaction.Service
{
    public static class ObservableActorRefImplicitSenderExtensions
    {
        /// <summary>
        /// Asynchronously tells a message to an <see cref="IActorRef"/>.
        /// </summary>
        /// <param name="receiver">The actor who will receive the message.</param>
        /// <param name="message">The message.</param>
        /// <remarks>Will automatically resolve the current sender using the current <see cref="ActorCell"/>, if any.</remarks>
        public static void TellObservable<TMessage>(this IActorRef receiver, TMessage message) where TMessage : notnull
        {
            var activity = Activity.Current;

            ActorRefImplicitSenderExtensions.Tell(receiver, new ObservableEnvelope<TMessage>(message, activity.Context));
        }


        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="self">TBD</param>
        /// <param name="message">TBD</param>

        /// <returns>TBD</returns>
        public static async Task<TResult> AskObservable<TResult, TMessage>(this ICanTell self, TMessage message) where TMessage : notnull
        {
            var activity = Activity.Current;

            var res = await self.Ask<ObservableEnvelope<TResult>>(new ObservableEnvelope<TMessage>(message, activity.Context));

            return res.Body;
        }

        //when asking from outside of an actor, we need to pass a system, so the FutureActor can register itself there and be resolvable for local and remote calls
        /// <summary>
        /// TBD
        /// </summary>
        /// <param name="self">TBD</param>
        /// <param name="message">TBD</param>
        /// <param name="timeout">TBD</param>
        /// <returns>TBD</returns>
        public static async Task<object> AskObservable<TMessage>(this ICanTell self, TMessage message) where TMessage : notnull
        {
            var activity = Activity.Current;
            var res = await self.Ask<ObservableEnvelope>(new ObservableEnvelope<TMessage>(message, activity.Context));

            return res.Body;
        }
    }
}