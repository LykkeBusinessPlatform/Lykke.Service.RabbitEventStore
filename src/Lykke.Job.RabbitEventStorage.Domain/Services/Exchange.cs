namespace Lykke.Job.RabbitEventStorage.Domain.Services
{
    public class ExchangeEntity
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }

    public class BindingEntity
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string RoutingKey { get; set; }
        public string Vhost { get; set; }
        public string DestinationType { get; set; }
    }
}
