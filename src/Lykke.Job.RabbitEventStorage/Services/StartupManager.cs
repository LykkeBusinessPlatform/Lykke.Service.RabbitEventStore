using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Sdk;
using Lykke.Job.RabbitEventStorage.Domain.Services;

namespace Lykke.Job.RabbitEventStorage.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly IRabbitService _rabbitService;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _rabbitMqSettings;

        public StartupManager(
            ILogFactory logFactory,
            IRabbitService rabbitService,
            RabbitMqSettings rabbitMqSettings)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
            _rabbitService = rabbitService;
            _rabbitMqSettings = rabbitMqSettings;
        }

        public async Task StartAsync()
        {
            var exchanges = await _rabbitService.GetAllExchangesAsync();
            var list = new List<RabbitMqSubscriber<string>>();
            exchanges = exchanges.Where(x => Regex.IsMatch(x.Name, _rabbitMqSettings.ExchangeRegex)).ToList();

            foreach (var exchange in exchanges)
            {
                var settings = RabbitMqSubscriptionSettings
                    .ForSubscriber(_rabbitMqSettings.ConnectionString,
                        exchange.Name,
                        "rabbiteventstoragejob")
                    .MakeDurable();

                var func = new Func<string, Task>(x => _rabbitService.SaveMessageAsync(
                    new RabbitMessage
                    {
                        ExchangeName = exchange.Name,
                        Payload = x
                    }));

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

            //await _rabbitService.RemoveSubscriptionsAsync();
        }
    }
}
