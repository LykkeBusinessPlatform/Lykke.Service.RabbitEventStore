using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.RabbitEventStorage.Domain.Services
{
    public interface IRabbitService
    {
        Task<IEnumerable<Exchange>> GetAllExchangesAsync();

        Task SaveMessageAsync(string exchangeName, string messagePayload);

        Task RemoveSubscriptionsAsync();
    }
}
