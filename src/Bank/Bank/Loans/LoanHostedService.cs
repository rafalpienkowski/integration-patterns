using System;
using System.Threading;
using System.Threading.Tasks;
using IntegrationFramework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RandomNameGeneratorLibrary;

namespace Bank.Loans
{
    public class LoanHostedService : IHostedService
    {
        private readonly ILogger<LoanHostedService> _logger;
        private const string RequestQueueName = "loan-quote-request";
        private const string ResponseQueueName = "loan-quote-response";
        private readonly IMessageBroker _messageBroker;
        private readonly Func<LoanQuoteRequest, LoanQuoteResponse> _onMessage;
        private readonly Random _random = new Random();
        private const double RatePremium = 1.5d;
        private readonly string _bankName;

        public LoanHostedService(ILogger<LoanHostedService> logger, IMessageBroker messageBroker)
        {
            _logger = logger;
            _messageBroker = messageBroker;
            
            var personGenerator = new PersonNameGenerator(_random);
            _bankName = personGenerator.GenerateRandomFirstName();


            _onMessage = request =>
            {
                ArbitraryWait();
                
                var primeRate = _random.NextDouble() * 5.0d;
                
                var interestRate =
                    primeRate + RatePremium + (double)(request?.LoanDuration ?? 1 / 12) / 10 +
                    _random.NextDouble();

                return new LoanQuoteResponse
                {
                    LoanInterest = interestRate,
                    QuoteName = $"{_bankName}'s Bank"
                };
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting bank loan service....");
            
            _messageBroker.Subscribe(RequestQueueName, ResponseQueueName, $"{_bankName.ToLowerInvariant()}-bank-queue",_onMessage);
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping bank loan service....");
            
            _messageBroker?.Dispose();
            
            return Task.CompletedTask;
        }

        private void ArbitraryWait()
        {
            var wait = _random.Next(1, 15);
            Thread.Sleep(wait * 1000);
        }
    }
}