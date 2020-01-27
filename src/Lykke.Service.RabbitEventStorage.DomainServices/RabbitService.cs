using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Lykke.Service.RabbitEventStorage.Domain.Services;
using Newtonsoft.Json;

namespace Lykke.Service.RabbitEventStorage.DomainServices
{
    public class RabbitService : IRabbitService
    {
        private readonly RabbitMqManagmentApiClient rabbitMqManagmentApiClient;

        public RabbitService(RabbitMqManagmentApiClient rabbitMqManagmentApiClient)
        {
            this.rabbitMqManagmentApiClient = rabbitMqManagmentApiClient;
        }

        public async Task<IEnumerable<Exchange>> GetAllExchanges()
        {
            var allExchanges = await rabbitMqManagmentApiClient.GetExchangesAsync();

            return allExchanges.Select(x => new Exchange() {Name = x.Name, Type = x.Type,});
        }
    }

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
            return await DoGetCall<IEnumerable<ExchangeResponse>>("api/exchanges");
        }

        private async Task<T> DoGetCall<T>(string path)
        {
            return await DoCall<T>(path, HttpMethod.Get);
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
}
