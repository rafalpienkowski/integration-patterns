using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AppointmentEnricher.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AppointmentEnricher
{
    public class AppointmentEnricherHostingService : IHostedService, IDisposable
    {
        private const string InQueueName = "scheduling-appointment-ended";
        private const string OutQueueName = "accounting-appointment-ended";
        private readonly string _rabbitHostName;
        private readonly HttpClient _httpClient = new HttpClient();
        private readonly ILogger<AppointmentEnricherHostingService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _customerCareUrl;

        public AppointmentEnricherHostingService(
            IOptions<Messaging> messaging, 
            IOptions<CustomerCare> customerCare, 
            ILogger<AppointmentEnricherHostingService> logger
        )
        {
            _rabbitHostName = messaging.Value.HostName;
            _customerCareUrl = customerCare.Value.Url;
            _logger = logger;
            
            var factory = new ConnectionFactory() { HostName = _rabbitHostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Appointment enricher start listening for {_rabbitHostName}...");
            
            _channel.QueueDeclare(queue: InQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($" [x] Received {message}");

                var succeeded = true;
                try
                {
                    await EnrichMessage(message);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unable to enrich the message");
                    _channel.BasicNack(ea.DeliveryTag, false, true);
                    succeeded = false;
                }

                if (succeeded)
                {
                    _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                }
            };
            
            _channel.BasicConsume(queue: InQueueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Appointment enricher stop listening for {_rabbitHostName}...");
            
            _channel?.Close();
            _connection?.Close();
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        
        private async Task EnrichMessage(string message)
        {
            var appointmentEnded = JsonConvert.DeserializeObject<AppointmentEndedEnriched>(message);
            appointmentEnded.CustomerAddress = await GetCustomerAddress(appointmentEnded.CustomerId);

            var messageEnriched = JsonConvert.SerializeObject(appointmentEnded);
            var messageBytes = Encoding.UTF8.GetBytes(messageEnriched);
            
            SendMessage(messageBytes);
        }
        
        private async Task<string> GetCustomerAddress(Guid customerId)
        {
            var responseMessage = await _httpClient.GetAsync($"{_customerCareUrl}/{customerId}/address");
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new ExternalException("Customer care not available");
            }

            var address = await responseMessage.Content.ReadAsStringAsync();

            return address;
        }
        
        private void SendMessage(byte[] message)
        {
            _channel.QueueDeclare(queue: OutQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicPublish(exchange: "",
                routingKey: OutQueueName,
                basicProperties: null,
                body: message);
            
            _logger.LogInformation($"Enriched message send");
        }
    }
}