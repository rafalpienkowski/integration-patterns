using System;
using System.Threading;
using System.Threading.Tasks;
using IntegrationFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CreditBureau.CreditScore
{
    public class CreditBureauHostedService : BackgroundService
    {
        private readonly ILogger<CreditBureauHostedService> _logger;
        private const string RequestQueueName = "credit-request";
        private readonly IMessageBroker _messageBroker;
        private readonly Func<CreditScoreRequest, CreditScoreResponse> _onMessage; 

        public CreditBureauHostedService(
            ILogger<CreditBureauHostedService> logger, 
            IMessageBroker messageBroker)
        {
            _logger = logger;
            _messageBroker = messageBroker;

            _onMessage = request =>
            {
                _logger.LogInformation($"Credit request message for {request.UserName} received");
                
                var random = new Random(request.UserName.Length);
                var response = new CreditScoreResponse
                {
                    UserName = request.UserName,
                    CreditScore = random.Next(600) + 300,
                    HistoryLenght = random.Next(19) + 1
                };

                _logger.LogInformation($"Credit response for {request.UserName} send: {JsonConvert.SerializeObject(response)}");
                
                return response;
            };
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting credit bureau");
            
            _messageBroker.StartListening(RequestQueueName, _onMessage);

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping credit bureau");
            
            _messageBroker?.Dispose();

            await base.StopAsync(cancellationToken);
        }
    }
}