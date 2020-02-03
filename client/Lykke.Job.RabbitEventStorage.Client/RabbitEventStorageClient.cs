using Lykke.HttpClientGenerator;

namespace Lykke.Job.RabbitEventStorage.Client
{
    /// <summary>
    /// RabbitEventStorage API aggregating interface.
    /// </summary>
    public class RabbitEventStorageClient : IRabbitEventStorageClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to RabbitEventStorage Api.</summary>
        public IRabbitEventStorageApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public RabbitEventStorageClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IRabbitEventStorageApi>();
        }
    }
}
