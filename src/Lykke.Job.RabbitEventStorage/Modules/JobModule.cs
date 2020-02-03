using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.Job.RabbitEventStorage.PeriodicalHandlers;
using Lykke.Job.RabbitEventStorage.RabbitPublishers;
using Lykke.Job.RabbitEventStorage.RabbitSubscribers;
using Lykke.Job.RabbitEventStorage.Services;
using Lykke.Job.RabbitEventStorage.Settings;
using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Job.RabbitEventStorage.Domain.Services;
using Lykke.Job.RabbitEventStorage.DomainServices;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.RabbitEventStorage.Modules
{
    public class JobModule : Module
    {
        private readonly RabbitEventStorageJobSettings _settings;
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.RabbitEventStorageJob;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>()
                .WithParameter("rabbitMqSettings", _settings.Rabbit)
                .SingleInstance();

            builder.RegisterType<RabbitService>()
                .As<IRabbitService>()
                .WithParameter("queueNameIdentifier", "rabbiteventstoragejob")
                .SingleInstance();

            builder.RegisterInstance(
                new RabbitMqManagmentApiClient(_settings.Rabbit.ManagementUrl,
                _settings.Rabbit.Username, 
                _settings.Rabbit.Password));

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>()
                .AutoActivate()
                .SingleInstance();

            RegisterPeriodicalHandlers(builder);

            RegisterRabbitMqSubscribers(builder);

            RegisterRabbitMqPublishers(builder);

            builder.Populate(_services);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<RestoreHandler>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            builder.RegisterType<RestoreRabbitPublisher>()
                .As<IRestoreRabbitPublisher>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))
                .AutoActivate();
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            builder.RegisterType<RestoreRabbitSubscriber>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance()
                .WithParameter("connectionString", _settings.Rabbit.ConnectionString)
                .WithParameter("exchangeName", "rabbiteventstoragejob.restore")
                .AutoActivate();
        }
    }
}
