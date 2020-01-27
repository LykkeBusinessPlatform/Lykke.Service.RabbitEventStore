using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Deduplication;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Sdk;
using Lykke.Service.RabbitEventStorage.Domain.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace Lykke.Job.RabbitEventStorage.Services
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.

    public class StartupManager : IStartupManager
    {
        private const string RABBIT_EVENT_STORAGE_SOURCE = "rabbiteventstoragesource";
        private readonly ILog _log;
        //private readonly TriggerHost _triggerHost;
        private readonly IRabbitService _rabbitService;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public StartupManager(
            ILogFactory logFactory,
            //TriggerHost triggerHost,
            IRabbitService rabbitService,
            RabbitMqSettings rabbitMqSettings)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
            //_triggerHost = triggerHost;
            _rabbitService = rabbitService;
            _rabbitMqSettings = rabbitMqSettings;
        }

        public async Task StartAsync()
        {
            HashSet<string> exclude = new HashSet<string>()
            {
                "",
                "amq.direct",
                "amq.fanout",
                "amq.headers",
                "amq.match",
                "amq.rabbitmq.trace",
                "amq.topic",
            };
            var exchanges = await _rabbitService.GetAllExchangesAsync();
            var list = new List<RabbitMqSubscriber<string>>();
            var deduplicator = new HeaderDeduplicator(RABBIT_EVENT_STORAGE_SOURCE);
            exchanges = exchanges.Where(x => !exclude.Contains(x.Name)).ToList().Take(1);

            foreach (var exchange in exchanges)
            {
                var settings = RabbitMqSubscriptionSettings
                    .ForSubscriber(_rabbitMqSettings.ConnectionString, 
                        exchange.Name, 
                        "rabbiteventstoragejob")
                    .MakeDurable();
                // TODO: Make additional configuration, using fluent API here:
                // ex: .MakeDurable()

                var func = new Func<string, Task>(x => { return _rabbitService.SaveMessageAsync(exchange.Name, x); });
                var subscriber = new RabbitMqSubscriber<string>(
                        _logFactory,
                        settings,
                        new ResilientErrorHandlingStrategy(
                            _logFactory,
                            settings,
                            TimeSpan.FromSeconds(10),
                            next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<string>())
                    .SetHeaderDeduplication(RABBIT_EVENT_STORAGE_SOURCE)
                    .SetDeduplicator(deduplicator)
                    .Subscribe(func)
                    .SetAlternativeExchange(_rabbitMqSettings.ConnectionString)
                    .CreateDefaultBinding()
                    .Start();

                list.Add(subscriber);
            }

            //await _rabbitService.RemoveSubscriptionsAsync();
        }

        /// <summary>
        /// Publish strategy for fanout exchange.
        /// </summary>
        public class CustomFanoutPublishStrategy : IRabbitMqPublishStrategy
        {
            private readonly bool _durable;
            private readonly string _header;
            private readonly BasicProperties _basicProperties;

            public CustomFanoutPublishStrategy(RabbitMqSubscriptionSettings settings, string header)
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                _header = header;
                _durable = settings.IsDurable;
                _basicProperties = new BasicProperties()
                {
                    Headers = new Dictionary<string, object>() {{_header, _header}}
                };
            }

            public void Configure(RabbitMqSubscriptionSettings settings, IModel channel)
            {
                channel.ExchangeDeclare(exchange: settings.ExchangeName, type: "fanout", durable: _durable);
            }

            public void Publish(RabbitMqSubscriptionSettings settings, IModel channel, RawMessage message)
            {
                channel.BasicPublish(
                    exchange: settings.ExchangeName,
                    // routingKey can't be null - I consider this as a bug in RabbitMQ.Client
                    routingKey: string.Empty,
                    basicProperties: _basicProperties,
                    body: message.Body);
            }
        }

        public class CustomPublishStrategy : IRabbitMqPublishStrategy
        {
            private readonly bool _durable;
            private readonly string _routingKey;
            private readonly string _header;
            private readonly BasicProperties _basicProperties;

            public CustomPublishStrategy(RabbitMqSubscriptionSettings settings, string header)
            {
                if (settings == null)
                    throw new ArgumentNullException(nameof(settings));

                _durable = settings.IsDurable;
                _routingKey = settings.RoutingKey ?? string.Empty;
                _header = header;
                _basicProperties = new BasicProperties()
                {
                    Headers = new Dictionary<string, object>() {{_header, _header}}
                };
            }

            public void Configure(RabbitMqSubscriptionSettings settings, IModel channel)
            {
                channel.ExchangeDeclare(exchange: settings.ExchangeName, type: "direct", durable: _durable);
            }

            public void Publish(RabbitMqSubscriptionSettings settings, IModel channel, RawMessage message)
            {
                channel.BasicPublish(
                    exchange: settings.ExchangeName,
                    routingKey: message.RoutingKey ?? _routingKey,
                    basicProperties: _basicProperties,
                    body: message.Body);
            }
        }

        public class HeaderDeduplicator : IDeduplicator
        {
            private readonly byte[] _headerValue;

            public HeaderDeduplicator(string header)
            {
                var bytes = Encoding.UTF8.GetBytes(header);
                var result =  System.Convert.ToBase64String(bytes);

                _headerValue = Encoding.UTF8.GetBytes(result.ToJson());
            }

            public Task<bool> EnsureNotDuplicateAsync(byte[] value)
            {
                var result = !Enumerable.SequenceEqual(value, _headerValue);

                return Task.FromResult(result);
            }
        }
    }
}
