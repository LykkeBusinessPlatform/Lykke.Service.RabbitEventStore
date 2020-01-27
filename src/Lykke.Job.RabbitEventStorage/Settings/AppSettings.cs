using Lykke.Job.RabbitEventStorage.Settings.JobSettings;
using Lykke.Sdk.Settings;

namespace Lykke.Job.RabbitEventStorage.Settings
{
    public class AppSettings : BaseAppSettings
    {
        public RabbitEventStorageJobSettings RabbitEventStorageJob { get; set; }
    }
}
