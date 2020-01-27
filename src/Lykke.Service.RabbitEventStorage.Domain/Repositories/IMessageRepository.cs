using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Service.RabbitEventStorage.Domain.Repositories
{
    public interface IMessageRepository
    {
        Task SaveAsync(string exchangeName, string messagePayload);
    }
}
