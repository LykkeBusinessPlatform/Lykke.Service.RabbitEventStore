using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.IncomingMessages;
using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.JobTriggers.Triggers;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Sdk;
using Lykke.Service.RabbitEventStorage.Domain.Repositories;
using Lykke.Service.RabbitEventStorage.Domain.Services;

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
        private readonly ILog _log;
        private readonly TriggerHost _triggerHost;
        private readonly IRabbitService _rabbitService;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;
        private readonly IMessageRepository _messageRepository;

        public StartupManager(
            ILogFactory logFactory,
            TriggerHost triggerHost,
            IRabbitService rabbitService,
            IMessageRepository messageRepository,
            RabbitMqSettings rabbitMqSettings)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
            _triggerHost = triggerHost;
            _rabbitService = rabbitService;
            _rabbitMqSettings = rabbitMqSettings;
            _messageRepository = messageRepository;
        }

        public async Task StartAsync()
        {
            var exchanges = await _rabbitService.GetAllExchanges();
            var list = new List<RabbitMqSubscriber<string>>();

            foreach (var exchange in exchanges)
            {
                var settings = RabbitMqSubscriptionSettings
                    .CreateForSubscriber(_rabbitMqSettings.ConnectionString, exchange.Name, "rabbiteventstoragejob")
                    .MakeDurable();
                // TODO: Make additional configuration, using fluent API here:
                // ex: .MakeDurable()

                var func = new Func<string, Task>(x => { return _messageRepository.SaveAsync(exchange.Name, x); });
                var subscriber = new RabbitMqSubscriber<string>(
                        _logFactory,
                        settings,
                        new ResilientErrorHandlingStrategy(
                            _logFactory,
                            settings,
                            TimeSpan.FromSeconds(10),
                            next: new DeadQueueErrorHandlingStrategy(_logFactory, settings)))
                    .SetMessageDeserializer(new JsonMessageDeserializer<string>())
                    .Subscribe(func)
                    .CreateDefaultBinding()
                    .Start();

                list.Add(subscriber);
            }
        }

    }
}
