using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.RabbitEventStorage.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public RabbitEventStorageSettings RabbitEventStorageService { get; set; }
    }
}
