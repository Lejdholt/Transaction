using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using Microsoft.Extensions.Hosting;
using Transaction.Service.Account;

namespace Transaction.Service
{
    public class AkkaService : IHostedService
    {
    
        private IActorRef _accouting;
        private IActorRef _kafka;

        public static ActorSystem Sys { get; private set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("app.conf"));
            Sys = ActorSystem.Create("bank", config);

            _kafka = Sys.ActorOf(Props.Create(() => new KafkaActor()), "kafka");
            _accouting = Sys.ActorOf(Props.Create(() => new AccountingActor(_kafka)), "accounting");
        }


        public Task StopAsync(CancellationToken cancellationToken) => CoordinatedShutdown.Get(Sys).Run(CoordinatedShutdown.ClrExitReason.Instance);

        public void StartTransaction(Account.Transaction transaction)
        {
            var actor = Sys.ActorOf(Props.Create(() => new TransactionActor(_accouting)), $"transaction_{transaction.Id.Id}");
            actor.TellObservable(transaction);
        }

        public async Task OpenAccount(OpenAccount openAccount)
        {
            await _accouting.AskObservable(openAccount);
        }
    }
}