using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.RabbitEventStorage.Settings.JobSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
