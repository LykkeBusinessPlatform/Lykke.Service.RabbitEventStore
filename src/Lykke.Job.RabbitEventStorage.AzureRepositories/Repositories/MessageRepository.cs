using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Job.RabbitEventStorage.AzureRepositories.Entities;
using Lykke.Job.RabbitEventStorage.Domain.Repositories;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Lykke.Job.RabbitEventStorage.AzureRepositories.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private static char[] _splittingChars = new char[] { '_' };
        private readonly INoSQLTableStorage<MessageEntity> _storage;

        public MessageRepository(INoSQLTableStorage<MessageEntity> storage)
        {
            _storage = storage;
        }

        public static string GetPartitionKey(string exchangeName, DateTime date)
        {
            return $"{exchangeName}_{date.Date:MM.dd.yyyy}";
        }

        public async Task SaveAsync(string exchangeName, DateTime date, long timestamp,string messagePayload)
        {
            var message = new MessageEntity(GetPartitionKey(exchangeName, date), timestamp.ToString()) {MessagePayload = messagePayload};

            await _storage.InsertAsync(message);
        }

        public async Task<(string ContinuationToken, IEnumerable<(string ExchangeName, string MessagePayload)> Messages)> 
            GetAsync(string exchangeName, DateTime date, int take = 100, string continuationToken = null)
        { 
            var result = await _storage.GetDataWithContinuationTokenAsync(
                GetPartitionKey(exchangeName, date), 
                take, 
                continuationToken);

            return (result.ContinuationToken, result.Entities.Select(x => 
                (x.PartitionKey.Split(_splittingChars,StringSplitOptions.RemoveEmptyEntries).FirstOrDefault(), 
                    x.MessagePayload)));
        }
    }
}
