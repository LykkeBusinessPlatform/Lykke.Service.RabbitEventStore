using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.RabbitEventStorage.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task SaveAsync(string exchangeName, DateTime date, long timestamp, string messagePayload);

        Task<(string ContinuationToken, IEnumerable<(string ExchangeName, string MessagePayload)> Messages)>
            GetAsync(string exchangeName, DateTime date, int take = 100, string continuationToken = null);
    }
}
