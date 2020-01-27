using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.RabbitEventStorage.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }

        public string ManagementUrl { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
