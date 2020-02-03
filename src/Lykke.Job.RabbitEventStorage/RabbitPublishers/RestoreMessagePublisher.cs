using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Job.RabbitEventStorage.Domain.Services;

namespace Lykke.Job.RabbitEventStorage.RabbitPublishers
{
    public class RestoreRabbitPublisher : IRestoreRabbitPublisher
    {
        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private RabbitMqPublisher<RestoreMessage> _publisher;

        public RestoreRabbitPublisher(ILogFactory logFactory, string connectionString)
        {
            _logFactory = logFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .ForPublisher(_connectionString, "rabbiteventstoragejob.restore")
                .MakeDurable();

            _publisher = new RabbitMqPublisher<RestoreMessage>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<RestoreMessage>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .Start();
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }

        public async Task PublishAsync(RestoreMessage message)
        {
            await _publisher.ProduceAsync(message);
        }
    }
}
