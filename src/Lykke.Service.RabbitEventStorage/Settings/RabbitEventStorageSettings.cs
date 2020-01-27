using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.RabbitEventStorage.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class RabbitEventStorageSettings
    {
        public DbSettings Db { get; set; }
    }
}
