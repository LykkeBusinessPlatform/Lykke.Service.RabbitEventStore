using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Lykke.Job.RabbitEventStorage.Domain.Repositories;
using Lykke.Job.RabbitEventStorage.Domain.Services;

namespace Lykke.Job.RabbitEventStorage.DomainServices
{
    public class RabbitService : IRabbitService
    {
        private readonly RabbitMqManagmentApiClient _rabbitMqManagementApiClient;
        private readonly string _queueNameIdentifier;
        private readonly IMessageRepository _messageRepository;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1);
        private ILookup<string, BindingEntity> _bindingsLookup;

        public RabbitService(
            RabbitMqManagmentApiClient rabbitMqManagementApiClient,
            IMessageRepository messageRepository,
                string queueNameIdentifier)
        {
            _queueNameIdentifier = queueNameIdentifier;
            _rabbitMqManagementApiClient = rabbitMqManagementApiClient;
            _messageRepository = messageRepository;
        }

        public async Task<IEnumerable<ExchangeEntity>> GetAllExchangesAsync()
        {
            var allExchanges = await _rabbitMqManagementApiClient.GetExchangesAsync();

            return allExchanges.Select(x => new ExchangeEntity() { Name = x.Name, Type = x.Type, });
        }
        
        public async Task<ILookup<string, BindingEntity>> GetAllBindingsAsync()
        {
            if (_bindingsLookup != null)
                return _bindingsLookup;
            
            try
            {
                await _lock.WaitAsync();
                
                if (_bindingsLookup == null)
                {
                    var allExchanges = await _rabbitMqManagementApiClient.GetBindingsAsync();

                    _bindingsLookup = allExchanges.Select(x => new BindingEntity
                    {
                        Source = x.Source,
                        Destination = x.Destination,
                        DestinationType = x.DestinationType,
                        Vhost = x.Vhost,
                        RoutingKey = x.RoutingKey
                    }).ToLookup(x => x.Source);
                }
            }
            finally
            {
                _lock.Release();
            }

            return _bindingsLookup;
        }


        public async Task SaveMessageAsync(RabbitMessage message)
        {
            var date = DateTime.UtcNow.Date;
            var timestamp = (long)DateTime.UtcNow.ToUnixTime();
            await _messageRepository.SaveAsync(message.ExchangeName, date, timestamp, message.Payload);
        }

        public async Task<(IEnumerable<RabbitMessage> Messages, string ContinuationToken)>
            RestoreMessageAsync(string exchangeName, DateTime date, int take, string continuationToken = null)
        {
            var result = await _messageRepository.GetAsync(exchangeName, date, take, continuationToken);

            return (result.Messages.Select(x => new RabbitMessage()
            {
                ExchangeName = x.ExchangeName,
                Payload = x.MessagePayload
            }), result.ContinuationToken);
        }

        public async Task RemoveSubscriptionsAsync()
        {
            var allQueues = await _rabbitMqManagementApiClient.GetQueuesAsync();
            var queuesForService = allQueues.Where(x => x.Name.Contains(_queueNameIdentifier));

            var tasks = new List<Task>(10);
            foreach (var queue in queuesForService)
            {
                tasks.Add(_rabbitMqManagementApiClient.RemoveQueueAsync(queue.Vhost, queue.Name));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
