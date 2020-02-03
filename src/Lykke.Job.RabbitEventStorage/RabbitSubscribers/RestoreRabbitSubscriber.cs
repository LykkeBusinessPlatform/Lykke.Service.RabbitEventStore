using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Contract;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Job.RabbitEventStorage.Domain.Services;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;

namespace Lykke.Job.RabbitEventStorage.RabbitSubscribers
{
    public class RestoreRabbitSubscriber : IStartable, IStopable
    {
        private readonly ILogFactory _logFactory;
        private readonly string _connectionString;
        private readonly string _exchangeName;
        private RabbitMqSubscriber<RestoreMessage> _subscriber;
        private readonly IRabbitService _rabbitService;
        private readonly JsonMessageSerializer<string> _messageSerializer;
        private RetryPolicy _retryPolicy;

        public RestoreRabbitSubscriber(
            ILogFactory logFactory,
            IRabbitService rabbitService,
            string connectionString,
            string exchangeName)
        {
            _logFactory = logFactory;
            _rabbitService = rabbitService;
            _connectionString = connectionString;
            _exchangeName = exchangeName;
            _messageSerializer = new JsonMessageSerializer<string>();
            _retryPolicy = RetryPolicy
                .Handle<Exception>()
                .WaitAndRetryAsync(10, (i) =>
                {
                    int baseMs = 100;
                    var exponentialWaitMs = baseMs * Math.Pow(2, i);
                    return TimeSpan.FromMilliseconds(exponentialWaitMs);
                });
        }

        public void Start()
        {
            // NOTE: Read https://github.com/LykkeCity/Lykke.RabbitMqDotNetBroker/blob/master/README.md to learn
            // about RabbitMq subscriber configuration

            var settings = RabbitMqSubscriptionSettings
                .ForSubscriber(_connectionString, _exchangeName, "subscriber");

            _subscriber = new RabbitMqSubscriber<RestoreMessage>(
                    _logFactory,
                    settings,
                    new ResilientErrorHandlingStrategy(
                        _logFactory,
                        settings,
                        TimeSpan.FromSeconds(10),
                        next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                .SetMessageDeserializer(new JsonMessageDeserializer<RestoreMessage>())
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        private async Task ProcessMessageAsync(RestoreMessage restoreMessage)
        {
            var currentDate = restoreMessage.FromDate.ToUniversalTime();
            var to = restoreMessage.ToDate.ToUniversalTime();
            var bindings = await _rabbitService.GetAllBindingsAsync();
            var exchangeBindings =
                bindings[restoreMessage.ExchangeName]
                    .Where(x => x.DestinationType == "queue")
                    .ToList();

            while (currentDate.Date <= to.Date)
            {
                string continuationToken = null;

                do
                {
                    var result =
                        await _rabbitService.RestoreMessageAsync(restoreMessage.ExchangeName, currentDate, 100,
                            continuationToken);

                    var factory = new ConnectionFactory() {Uri = _connectionString,};
                    foreach (var binding in exchangeBindings)
                    {
                        using (var connection = factory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            foreach (var message in result.Messages)
                            {
                                //channel.QueueDeclare(queue: "hello",
                                //    durable: false,
                                //    exclusive: false,
                                //    autoDelete: false,
                                //    arguments: null);

                                var body = _messageSerializer.Serialize(message.Payload);

                                channel.BasicPublish(exchange: "",
                                    routingKey: binding.Destination,
                                    basicProperties: null,
                                    body: body);
                            }
                        }

                        continuationToken = result.ContinuationToken;
                    }

                    currentDate = currentDate.AddDays(1.0);
                } while (!string.IsNullOrEmpty(continuationToken));
            }
        }

        public void Dispose()
            {
                _subscriber?.Dispose();
            }

            public void Stop()
            {
                _subscriber?.Stop();
            }
        }
    }
