using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Accounting.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Accounting
{
    public class AccountingHostedService : IHostedService, IDisposable
    {
        private readonly string _rabbitHostName;
        private readonly ILogger<AccountingHostedService> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string QueueName = "accounting-appointment-ended";

        public AccountingHostedService(
            IOptions<Messaging> messaging, 
            ILogger<AccountingHostedService> logger
            )
        {
            _rabbitHostName = messaging.Value.HostName;
            _logger = logger;
            
            var factory = new ConnectionFactory() { HostName = _rabbitHostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Accounting service start listening for {_rabbitHostName}...");
            
            _channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message: {message}");

                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            
            _channel.BasicConsume(queue: QueueName,
                autoAck: false,
                consumer: consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Accounting service stop listening for {_rabbitHostName}...");
            _channel?.Close();
            _connection?.Close();
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}