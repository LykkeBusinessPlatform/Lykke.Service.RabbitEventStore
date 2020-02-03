using JetBrains.Annotations;

namespace Lykke.Job.RabbitEventStorage.Client
{
    /// <summary>
    /// RabbitEventStorage client interface.
    /// </summary>
    [PublicAPI]
    public interface IRabbitEventStorageClient
    {
        // Make your app's controller interfaces visible by adding corresponding properties here.
        // NO actual methods should be placed here (these go to controller interfaces, for example - IRabbitEventStorageApi).
        // ONLY properties for accessing controller interfaces are allowed.

        /// <summary>Application Api interface</summary>
        IRabbitEventStorageApi Api { get; }
    }
}
