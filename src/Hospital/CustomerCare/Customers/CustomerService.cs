using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace CustomerCare.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly CustomersContext _context;
        private readonly string _messagingHostName;
        private const string QueueName = "customer-care-customer-created";
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IOptions<Messaging> messaging, CustomersContext context, ILogger<CustomerService> logger)
        {
            _context = context;
            _logger = logger;
            _messagingHostName = messaging.Value?.HostName;
        }

        public async Task Add(Customer customer)
        {
            customer.Id = Guid.NewGuid();

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            
            SendMessage(customer);
        }

        public async Task<string> GetAddressFor(Guid customerId)
        {
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customerId);
            return customer == null 
                ? string.Empty 
                : customer.Address;
        }

        public async Task<List<Customer>> GetAll() => await _context.Customers.ToListAsync();

        private void SendMessage(Customer customer)
        {
            var factory = new ConnectionFactory
            {
                HostName = _messagingHostName
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            
            channel.QueueDeclare(queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = JsonSerializer.Serialize(new CreateCustomer
            {
                Id = customer.Id,
                Name = $"{customer.FirstName} {customer.LastName}"
            });
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                routingKey: QueueName,
                basicProperties: null,
                body: body);
            
            _logger.LogInformation("Customer created messaged published");
        }
    }
}