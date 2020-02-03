using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.RabbitEventStorage.Client 
{
    /// <summary>
    /// RabbitEventStorage client settings.
    /// </summary>
    public class RabbitEventStorageServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
