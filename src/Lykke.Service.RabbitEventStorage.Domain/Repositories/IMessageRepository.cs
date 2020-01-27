using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.RabbitEventStorage.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task SaveAsync(string exchangeName, long timestamp, string messagePayload);

        Task<(string ContinuationToken, IEnumerable<(string ExchangeName, string MessagePayload)> Messages)>
            GetAsync(string exchangeName, int take = 100, string continuationToken = null);
    }
}
