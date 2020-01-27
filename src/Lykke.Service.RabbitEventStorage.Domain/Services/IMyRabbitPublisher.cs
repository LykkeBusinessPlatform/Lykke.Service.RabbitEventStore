using System.Threading.Tasks;
using Autofac;
using Common;
using Lykke.Job.RabbitEventStorage.Contract;

namespace Lykke.Service.RabbitEventStorage.Domain.Services
{
    public interface IMyRabbitPublisher : IStartable, IStopable
    {
        Task PublishAsync(MyPublishedMessage message);
    }
}