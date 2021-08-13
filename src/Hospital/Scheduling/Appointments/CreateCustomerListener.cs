using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Scheduling.Appointments
{
    public class CreateCustomerListener : BackgroundService
    {
        private IServiceProvider Services { get; }
        private readonly ILogger<CreateCustomerListener> _logger;
        private readonly IModel _channel;
        private const string QueueName = "scheduling-create-customer";

        public CreateCustomerListener(IServiceProvider services, ILogger<CreateCustomerListener> logger, IOptions<Messaging> messaging)
        {
            Services = services;
            _logger = logger;
                
            var factory = new ConnectionFactory() { HostName = messaging.Value.HostName };
            var connection = factory.CreateConnection();
            _channel = connection.CreateModel();
            
            _channel.QueueDeclare(queue: QueueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Create customer listener running");
            stoppingToken.ThrowIfCancellationRequested();
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var customer = new CreateCustomerMessageWrapper(body);
                
                using var scope = Services.CreateScope();
                var appointmentsService = scope.ServiceProvider.GetRequiredService<IAppointmentsService>();
                var patient = new Patient
                {
                    Id = customer.Id,
                    Name = customer.Name
                };
                await appointmentsService.Add(patient);
                
                _logger.LogInformation($"New patient added: {JsonConvert.SerializeObject(patient)}");
            };
            
            _channel.BasicConsume(queue: QueueName,
                        autoAck: true,
                        consumer: consumer);
                    
            return Task.CompletedTask;
        }

    }
}