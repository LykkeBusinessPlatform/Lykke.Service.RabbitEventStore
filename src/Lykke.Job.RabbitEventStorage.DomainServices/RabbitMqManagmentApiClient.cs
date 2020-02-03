using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Lykke.Job.RabbitEventStorage.DomainServices
{
    public class RabbitMqManagmentApiClient
    {
        private readonly HttpClient _http;

        public RabbitMqManagmentApiClient(string rabbitMqUrl, string username, string password)
        {
            var basicAuthHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));
            _http = new HttpClient {Timeout = TimeSpan.FromMinutes(2)};
            _http.BaseAddress = new Uri(rabbitMqUrl);
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", basicAuthHeader);
        }

        public async Task<IEnumerable<ExchangeResponse>> GetExchangesAsync()
        {
            return await DoCall<IEnumerable<ExchangeResponse>>("api/exchanges", HttpMethod.Get);
        }

        public async Task<IEnumerable<Binding>> GetBindingsAsync()
        {
            return await DoCall<IEnumerable<Binding>>("api/bindings", HttpMethod.Get);
        }

        // <summary>
        /// A list of all queues.
        /// </summary>
        public async Task<IEnumerable<Queue>> GetQueuesAsync()
        {
            return await DoCall<IEnumerable<Queue>>("/api/queues", HttpMethod.Get);
        }

        public async Task<IEnumerable<Queue>> RemoveQueueAsync(string vhost, string queueName)
        {
            return await DoCall<IEnumerable<Queue>>($"/api/queues/{vhost}/{queueName}", HttpMethod.Delete);
        }

        private async Task<T> DoCall<T>(string path, HttpMethod method, dynamic body = null)
        {
            HttpResponseMessage response;
            if (method == HttpMethod.Get)
            {
                response = await _http.GetAsync(path);
            }
            else if (method == HttpMethod.Post)
            {
                string messageBodyContent = JsonConvert.SerializeObject(body);
                response = await _http.PostAsync(path, new StringContent(messageBodyContent));
            }
            else if (method == HttpMethod.Put)
            {
                string messageBodyContent = JsonConvert.SerializeObject(body);
                response = await _http.PutAsync(path, new StringContent(messageBodyContent));
            }
            else
            {
                throw new Exception("method not implemented");
            }

            string result = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
            else
            {
                throw new HttpRequestException(result);
            }
        }
    }

    //TODO: Make an interface out of it

    public class ExchangeResponse
    {
        [JsonProperty("message_stats")]
        public MessageStats MessageStats { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("vhost")]
        public string Vhost { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("durable")]
        public bool Durable { get; set; }

        [JsonProperty("auto_delete")]
        public bool AutoDelete { get; set; }

        [JsonProperty("internal")]
        public bool Internal { get; set; }

        [JsonIgnore]
        [JsonProperty("arguments")]
        public Arguments Arguments { get; set; }
    }

    public class MessageStats
    {
        public long Publish { get; set; }
        public RateDetails PublishDetails { get; set; }

        public long PublishIn { get; set; }
        public RateDetails PublishInDetails { get; set; }

        public long PublishOut { get; set; }
        public RateDetails PublishOutDetails { get; set; }

        public long Ack { get; set; }
        public RateDetails AckDetails { get; set; }

        public long DeliverGet { get; set; }
        public RateDetails DeliverGetDetails { get; set; }

        public long Confirm { get; set; }
        public RateDetails ConfirmDetails { get; set; }

        public long ReturnUnroutable { get; set; }
        public RateDetails ReturnUnroutableDetails { get; set; }

        public long Redeliver { get; set; }
        public RateDetails RedeliverDetails { get; set; }
    }

    public class RateDetails
    {
        [JsonProperty("rate")]
        public double Rate { get; set; }
    }

    public class Arguments
    {
    }

    public class Queue
    {
        [JsonProperty("memory")]
        public long Memory { get; set; }

        [JsonProperty("reductions")]
        public long Reductions { get; set; }

        [JsonProperty("reductions_details")]
        public RateDetails ReductionsDetails { get; set; }

        [JsonProperty("messages")]
        public long Messages { get; set; }

        [JsonProperty("messages_details")]
        public RateDetails MessagesDetails { get; set; }

        [JsonProperty("messages_ready")]
        public long MessagesReady { get; set; }

        [JsonProperty("messages_ready_details")]
        public RateDetails MessagesReadyDetails { get; set; }

        [JsonProperty("messages_unacknowledged")]
        public long MessagesUnacknowledged { get; set; }

        [JsonProperty("messages_unacknowledged_details")]
        public RateDetails MessagesUnacknowledgedDetails { get; set; }

        [JsonProperty("idle_since")]
        public string IdleSince { get; set; }

        [JsonProperty("consumer_utilisation")]
        public double? ConsumerUtilisation { get; set; }

        [JsonProperty("policy")]
        public string Policy { get; set; }

        [JsonProperty("exclusive_consumer_tag")]
        public string ExclusiveConsumerTag { get; set; }

        [JsonProperty("consumers")]
        public int Consumers { get; set; }

        [JsonProperty("recoverable_slaves")]
        public object RecoverableSlaves { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("messages_ram")]
        public long MessagesRam { get; set; }

        [JsonProperty("messages_ready_ram")]
        public long MessagesReadyRam { get; set; }

        [JsonProperty("messages_unacknowledged_ram")]
        public long MessagesUnacknowledgedRam { get; set; }

        [JsonProperty("messages_persistent")]
        public long MessagesPersistent { get; set; }

        [JsonProperty("message_bytes")]
        public long MessageBytes { get; set; }

        [JsonProperty("message_bytes_ready")]
        public long MessageBytesReady { get; set; }

        [JsonProperty("message_bytes_unacknowledged")]
        public long MessageBytesUnacknowledged { get; set; }

        [JsonProperty("message_bytes_ram")]
        public long MessageBytesRam { get; set; }

        [JsonProperty("message_bytes_persistent")]
        public long MessageBytesPersistent { get; set; }

        [JsonProperty("head_message_timestamp")]
        public object HeadMessageTimestamp { get; set; }

        [JsonProperty("disk_reads")]
        public long DiskReads { get; set; }

        [JsonProperty("disk_writes")]
        public long DiskWrites { get; set; }

        [JsonProperty("node")]
        public string Node { get; set; }

        [JsonIgnore]
        [JsonProperty("arguments")]
        public Arguments Arguments { get; set; }

        [JsonProperty("exclusive")]
        public bool Exclusive { get; set; }

        [JsonProperty("auto_delete")]
        public bool AutoDelete { get; set; }

        [JsonProperty("durable")]
        public bool Durable { get; set; }

        [JsonProperty("vhost")]
        public string Vhost { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Binding
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("vhost")]
        public string Vhost { get; set; }

        [JsonProperty("destination")]
        public string Destination { get; set; }

        [JsonProperty("destination_type")]
        public string DestinationType { get; set; }

        [JsonProperty("routing_key")]
        public string RoutingKey { get; set; }

        [JsonIgnore]
        [JsonProperty("arguments")]
        public Arguments Arguments { get; set; }

        [JsonProperty("properties_key")]
        public string PropertiesKey { get; set; }
    }

    public enum ExchangeBindingType
    {
        Source,
        Destination
    }
}
