using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerCare.RecipientList.Configuration;
using Dapper;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CustomerCare.RecipientList
{
    public class RecipientListHostedService : IHostedService, IDisposable
    {
        private const string QueueName = "customer-care-customer-created";
        private readonly string _rabbitHostName;
        private readonly string _databaseConnectionString;
        private readonly ILogger<RecipientListHostedService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RecipientListHostedService(
            IOptions<Messaging> messaging,
            IOptions<Database> database,
            ILogger<RecipientListHostedService> logger
        )
        {
            _rabbitHostName = messaging.Value.HostName;
            _databaseConnectionString = database.Value.ConnectionString;
            _logger = logger;
            
            
            var factory = new ConnectionFactory() { HostName = _rabbitHostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Customer care recipient list start listening for {_rabbitHostName}...");
            
            _channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation($"Message received: {message}");
                
                var recipients = await GetRecipients();

                foreach (var recipient in recipients)
                {
                    SendMessage(recipient, body);
                }
                    
                //ACK after sending message to all recipient
                _channel.BasicAck(deliveryTag:ea.DeliveryTag, multiple: false);
            };
            
            _channel.BasicConsume(queue: QueueName,
                autoAck: false,
                consumer: consumer);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Customer care recipient list stop listening for {_rabbitHostName}...");
            
            _channel?.Close();
            _connection?.Close();
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
        
        private async Task<IEnumerable<Recipient>> GetRecipients()
        {
            await using var connection = new NpgsqlConnection(_databaseConnectionString);
            
            var recipients =
                await connection.QueryAsync<Recipient>(
                    "SELECT * FROM customercarerecipients.\"Recipients\";");

            return recipients;
        }
        
        private void SendMessage(Recipient recipient, byte[] message)
        {
            _channel.QueueDeclare(queue: recipient.Queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.BasicPublish(exchange: "",
                routingKey: recipient.Queue,
                basicProperties: null,
                body: message);
            
            _logger.LogInformation($"Message forwarded to: {recipient.Name}");
        }
    }
}