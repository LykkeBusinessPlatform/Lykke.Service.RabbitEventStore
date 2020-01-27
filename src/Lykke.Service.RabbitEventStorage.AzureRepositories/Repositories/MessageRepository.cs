using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.RabbitEventStorage.AzureRepositories.Entities;
using Lykke.Service.RabbitEventStorage.Domain.Repositories;

namespace Lykke.Service.RabbitEventStorage.AzureRepositories.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly INoSQLTableStorage<MessageEntity> _storage;

        public MessageRepository(INoSQLTableStorage<MessageEntity> storage)
        {
            _storage = storage;
        }

        public async Task SaveAsync(string exchangeName, long timestamp,string messagePayload)
        {
            var message = new MessageEntity(exchangeName, timestamp.ToString()) {MessagePayload = messagePayload};

            await _storage.InsertAsync(message);
        }

        public async Task<(string ContinuationToken, IEnumerable<(string ExchangeName, string MessagePayload)> Messages)> 
            GetAsync(string exchangeName, int take = 100, string continuationToken = null)
        { 
            var result = await _storage.GetDataWithContinuationTokenAsync(exchangeName, take, continuationToken);

            return (result.ContinuationToken, result.Entities.Select(x => (x.PartitionKey, x.MessagePayload)));
        }
    }
}
