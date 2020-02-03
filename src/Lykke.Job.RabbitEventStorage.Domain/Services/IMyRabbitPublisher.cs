using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Job.RabbitEventStorage.Contract;

namespace Lykke.Job.RabbitEventStorage.Domain.Services
{
    public interface IRestoreRabbitPublisher : IStartable, IStopable
    {
        Task PublishAsync(RestoreMessage message);
    }
}
