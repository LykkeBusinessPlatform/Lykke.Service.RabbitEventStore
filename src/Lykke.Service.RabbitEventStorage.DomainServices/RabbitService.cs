using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Lykke.Service.RabbitEventStorage.Domain.Repositories;
using Lykke.Service.RabbitEventStorage.Domain.Services;
using Newtonsoft.Json;

namespace Lykke.Service.RabbitEventStorage.DomainServices
{
    public class RabbitService : IRabbitService
    {
        private readonly RabbitMqManagmentApiClient _rabbitMqManagmentApiClient;
        private readonly string _queueNameIdentifier;
        private readonly IMessageRepository _messageRepository;

        public RabbitService(
            RabbitMqManagmentApiClient rabbitMqManagmentApiClient, 
            IMessageRepository messageRepository,
                string queueNameIdentifier)
        {
            this._queueNameIdentifier = queueNameIdentifier;
            this._rabbitMqManagmentApiClient = rabbitMqManagmentApiClient;
            this._messageRepository = messageRepository;
        }

        public async Task<IEnumerable<Exchange>> GetAllExchangesAsync()
        {
            var allExchanges = await _rabbitMqManagmentApiClient.GetExchangesAsync();

            return allExchanges.Select(x => new Exchange() {Name = x.Name, Type = x.Type,});
        }

        public async Task SaveMessageAsync(string exchangeName, string messagePayload)
        {
            var timestamp = (long)DateTime.UtcNow.ToUnixTime();
            await _messageRepository.SaveAsync(exchangeName, timestamp, messagePayload);
        }

        public async Task RemoveSubscriptionsAsync()
        {
            var allQueues = await _rabbitMqManagmentApiClient.GetQueuesAsync();
            var queuesForService = allQueues.Where(x => x.Name.Contains(_queueNameIdentifier));

            var tasks = new List<Task>(10);
            foreach (var queue in queuesForService)
            {
                tasks.Add(_rabbitMqManagmentApiClient.RemoveQueueAsync(queue.Vhost, queue.Name));
            }

            Task.WaitAll(tasks.ToArray());
        }
    }
}
