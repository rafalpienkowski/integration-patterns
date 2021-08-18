using System;
using System.Text;
using IntegrationFramework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace IntegrationFramework
{
    public class RabbitMq : IMessageBroker
    {
        private readonly MessageBrokerConfiguration _configuration;
        private readonly ILogger<RabbitMq> _logger;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RabbitMq(ILogger<RabbitMq> logger, IConfiguration config)
        {
            _configuration = config.GetSection("MessageBroker").Get<MessageBrokerConfiguration>();
            _logger = logger;

            IConnectionFactory connectionFactory = new ConnectionFactory
            {
                HostName = _configuration.HostName
            };
            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void StartListening<TIn, TOut>(string queueName, Func<TIn, TOut> onMessage)
            where TIn : class, new() where TOut : class, new()
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

            var consumer = OnMessage(ea => ea.BasicProperties.ReplyTo, ea =>
            {
                var props = _channel.CreateBasicProperties();
                props.CorrelationId = ea.BasicProperties.CorrelationId;

                return props;
            }, onMessage);
            
            _channel.BasicConsume(queue: queueName, autoAck: false, consumer);
            _logger.LogInformation($"Listening for messages. Waiting for request on {queueName}.");
        }

        public void Subscribe<TIn, TOut>(string inQueueName, string outQueueName, string bindQueueName, Func<TIn, TOut> onMessage) 
            where TIn : class, new() where TOut : class, new()
        {
            _channel.ExchangeDeclare(exchange: inQueueName, type: ExchangeType.Fanout, durable: true);
            var queueName = _channel.QueueDeclare(queue: bindQueueName, exclusive: false, durable: true, autoDelete: true).QueueName;
            
            _channel.QueueBind(queue: queueName, exchange: inQueueName, routingKey:"");

            var consumer = OnMessage(ea => outQueueName, ea =>
            {
                var props = _channel.CreateBasicProperties();
                props.CorrelationId = ea.BasicProperties.CorrelationId;

                return props;
            }, onMessage);

            _channel.BasicConsume(queue: queueName, autoAck: false, consumer);
            
            _logger.LogInformation($"Subscribed for messages. Waiting for request on {queueName}.");
        }

        public void SendMessage(string queueName, string replyTo, string correlationId, object message)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = replyTo;
            
            var messageBytes = SerializeMessage(message);

            _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: properties, body: messageBytes);
        }

        public void Publish(string queueName, string correlationId, object message)
        {
            _channel.ExchangeDeclare(exchange: queueName, type: ExchangeType.Fanout, durable: true);
            
            var messageBytes = SerializeMessage(message);
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;
            
            _channel.BasicPublish(exchange: queueName, routingKey: "", basicProperties: properties, body: messageBytes);

            _logger.LogInformation($"Message published to queue: {queueName}");
        }
        
        public void OnResponse<TIn>(string queueName, Action<TIn, MessageProperties> onMessage) where TIn : class, new()
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                try
                {
                    var request = DeserializeMessage<TIn>(ea.Body.ToArray());

                    onMessage(request, new MessageProperties { CorrelationId = ea.BasicProperties.CorrelationId });
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Something went wrong");
                }
            };
            
            _channel.BasicConsume(consumer, queueName, autoAck: true);
        }

        private EventingBasicConsumer OnMessage<TIn, TOut>(Func<BasicDeliverEventArgs, string> outQueueName,
            Func<BasicDeliverEventArgs, IBasicProperties> replyProperties, Func<TIn, TOut> handleMessage)
            where TIn : class, new() where TOut : class, new()
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var response = default(TOut);
                var props = ea.BasicProperties;

                _logger.LogInformation($"Message received. Correlation Id: {props.CorrelationId}");

                var failed = false;

                try
                {
                    var request = DeserializeMessage<TIn>(ea.Body.ToArray());

                    response = handleMessage(request);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Something went wrong");
                    failed = true;
                }
                finally
                {
                    if (failed)
                    {
                        _channel.BasicNack(ea.DeliveryTag, false, true);
                        _logger.LogError($"Message NACK....");
                    }
                    else
                    {
                        var replyProps = replyProperties(ea);
                        
                        var messageBytes = SerializeMessage(response);

                        _channel.BasicPublish(exchange: "", routingKey: outQueueName(ea), basicProperties: replyProps, body: messageBytes);
                        _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                        
                        _logger.LogInformation($"Response send. Queue: {outQueueName(ea)} Correlation Id: {replyProps.CorrelationId}");
                    }
                }
            };

            return consumer;
        }

        private static TIn DeserializeMessage<TIn>(byte[] body) where TIn : class, new() 
        {
            var message = Encoding.UTF8.GetString(body);
            var request = JsonConvert.DeserializeObject<TIn>(message);
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Unable to serialize request");
            }

            return request;
        }

        private static byte[] SerializeMessage(object message)
        {
            var messageString = JsonConvert.SerializeObject(message);
            var messageBytes = Encoding.UTF8.GetBytes(messageString);

            return messageBytes;
        }

        public void Dispose()
        {
            _logger.LogInformation($"Stop listening to {_configuration.HostName}....");
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}