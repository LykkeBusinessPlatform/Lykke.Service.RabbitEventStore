using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common;
using Lykke.Job.RabbitEventStorage.PeriodicalHandlers;
using Lykke.Job.RabbitEventStorage.Services;
using Lykke.Job.RabbitEventStorage.Settings;
using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.Sdk;
using Lykke.Sdk.Health;
using Lykke.Service.RabbitEventStorage.Domain.Services;
using Lykke.Service.RabbitEventStorage.DomainServices;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.RabbitEventStorage.Modules
{
    public class JobModule : Module
    {
        private readonly RabbitEventStorageJobSettings _settings;
        private readonly IReloadingManager<AppSettings> _settingsManager;
        private readonly IServiceCollection _services;

        public JobModule(IReloadingManager<AppSettings> settingsManager)
        {
            _settings = settingsManager.CurrentValue.RabbitEventStorageJob;
            _settingsManager = settingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

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

            //builder.RegisterType< FakeMessageRepo > ()
            //    .As<IMessageRepository>()
            //    .SingleInstance();

            RegisterPeriodicalHandlers(builder);

            RegisterRabbitMqSubscribers(builder);

            RegisterRabbitMqPublishers(builder);

            // TODO: Add your dependencies here

            builder.Populate(_services);
        }

        //public class FakeMessageRepo : IMessageRepository
        //{
        //    private readonly List<(string exchange, string payload)> _messages = new List<(string exchange, string payload)>();

        //    public Task SaveAsync(string exchangeName, string messagePayload)
        //    {
        //        _messages.Add((exchangeName, messagePayload));

        //        return Task.CompletedTask;
        //    }
        //}


        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            // TODO: You should register each periodical handler in DI container as IStartable singleton and autoactivate it

            builder.RegisterType<MyPeriodicalHandler>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
        }

        private void RegisterRabbitMqSubscribers(ContainerBuilder builder)
        {
            
        }

        private void RegisterRabbitMqPublishers(ContainerBuilder builder)
        {
            // TODO: You should register each publisher in DI container as publisher specific interface and as IStartable,
            // as singleton and do not autoactivate it

            //builder.RegisterJsonRabbitSubscriber

            //builder.RegisterType<MyRabbitPublisher>()
            //    .As<IMyRabbitPublisher>()
            //    .As<IStartable>()
            //    .SingleInstance()
            //    .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString));
        }
    }
}
