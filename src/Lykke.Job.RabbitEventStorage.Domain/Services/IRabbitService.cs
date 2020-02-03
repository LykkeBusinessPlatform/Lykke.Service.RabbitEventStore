using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.RabbitEventStorage.Domain.Services
{
    public interface IRabbitService
    {
        Task<IEnumerable<ExchangeEntity>> GetAllExchangesAsync();

        Task<ILookup<string, BindingEntity>> GetAllBindingsAsync();

        Task SaveMessageAsync(RabbitMessage message);

        Task<(IEnumerable<RabbitMessage> Messages, string ContinuationToken)>
            RestoreMessageAsync(string exchangeName, DateTime date, int take, string continuationToken = null);

        Task RemoveSubscriptionsAsync();
    }
}
