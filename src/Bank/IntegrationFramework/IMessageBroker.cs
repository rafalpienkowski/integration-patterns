using System;

namespace IntegrationFramework
{
    public interface IMessageBroker : IDisposable
    {
        /// <summary>
        /// Starts listening on queue. Performs function on incoming message and sends response to replyTo queue
        /// </summary>
        void StartListening<TIn, TOut>(string queueName, Func<TIn, TOut> onMessage)
            where TIn : class, new() where TOut : class, new();
        
        void Subscribe<TIn, TOut>(string inQueueName, string outQueueName, string bindQueueName, Func<TIn,TOut> onMessage)
            where TIn : class, new() where TOut : class, new();

        void SendMessage(string queueName, string replyTo, string correlationId, object message);

        void Publish(string queueName, string correlationId, object message);

        void OnResponse<TIn>(string queueName,  Action<TIn, MessageProperties> onMessage)
            where TIn : class, new();
    }
}