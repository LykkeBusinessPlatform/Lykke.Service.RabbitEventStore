using System;

namespace Lykke.Job.RabbitEventStorage.Contract
{
    // NOTE: This is incoming message example
    public class RestoreMessage
    {
        public string ExchangeName { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }
    }
}
