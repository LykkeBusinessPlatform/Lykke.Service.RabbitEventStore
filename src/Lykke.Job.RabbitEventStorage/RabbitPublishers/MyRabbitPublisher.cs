using System.Threading.Tasks;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Contract;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.RabbitEventStorage.Domain.Services;

namespace Lykke.Job.RabbitEventStorage.RabbitPublishers
{
    public class MyRabbitPublisher : IMyRabbitPublisher
    {
        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private RabbitMqPublisher<MyPublishedMessage> _publisher;

        public MyRabbitPublisher(ILogFactory logFactory, string connectionString)
        {
            _logFactory = logFactory;
            _connectionString = connectionString;
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_connectionString, "rabbiteventstoragejob");
            // TODO: Make additional configuration, using fluent API here:
            // ex: .MakeDurable()

            _publisher = new RabbitMqPublisher<MyPublishedMessage>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<MyPublishedMessage>())
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

        public async Task PublishAsync(MyPublishedMessage message)
        {
            await _publisher.ProduceAsync(message);
        }
    }
}
