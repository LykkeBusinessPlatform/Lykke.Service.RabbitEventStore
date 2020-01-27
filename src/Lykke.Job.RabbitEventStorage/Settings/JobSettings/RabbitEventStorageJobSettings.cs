namespace Lykke.Job.RabbitEventStorage.Settings.JobSettings
{
    public class RabbitEventStorageJobSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings Rabbit { get; set; }
    }
}
