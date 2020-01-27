using Autofac;
using AzureStorage.Tables;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.RabbitEventStorage.Settings;
using Lykke.Service.RabbitEventStorage.AzureRepositories.Entities;
using Lykke.Service.RabbitEventStorage.AzureRepositories.Repositories;
using Lykke.Service.RabbitEventStorage.Domain.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Job.RabbitEventStorage.Modules
{
    [UsedImplicitly]
    public class DbModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;
        private readonly string _connectionString;


        public DbModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
            _connectionString = appSettings.CurrentValue.RabbitEventStorageJob.Db.DataConnString;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<MessageRepository>()
                .As<IMessageRepository>()
                .SingleInstance();

            builder
                .Register(ctx =>
                AzureTableStorage<MessageEntity>.Create(
                    _appSettings.ConnectionString(x => x.RabbitEventStorageJob.Db.DataConnString),
                    "Messages",
                    ctx.Resolve<ILogFactory>()))
                .SingleInstance();
        }
    }
}
