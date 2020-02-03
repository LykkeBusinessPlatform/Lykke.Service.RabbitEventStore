using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.RabbitEventStorage.Domain.Services
{
    public class RabbitMessage
    {
        public string ExchangeName { get; set; }

        public string Payload { get; set; }
    }
}
