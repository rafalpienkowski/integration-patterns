using System;
using System.Threading.Tasks;
using CreditAgent.Agent.Messages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CreditAgent.Agent
{
    public class LoanHub : Hub
    {
        public const string HubUrl = "/loan";
        private readonly ILogger<LoanHub> _logger;

        public const string ReceiveCreditScore = "ReceiveCreditScore";
        public const string ReceiveBankQuote = "ReceiveBankQuote";
        public const string ReceiveBestQuote = "ReceiveBestQuote";

        public LoanHub(ILogger<LoanHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"{Context.ConnectionId} connected");

            await base.OnConnectedAsync();
        }

        public async Task SendCreditScore(string clientId, Guid loanProcessId, CreditBureauResponse score)
        {
            await Clients.Client(clientId).SendAsync(ReceiveCreditScore, loanProcessId, score);
        }

        public async Task SendBankQuote(string clientId, Guid loanProcessId, LoanQuoteResponse quote)
        {
            await Clients.Client(clientId).SendAsync(ReceiveBankQuote, loanProcessId, quote);
        }

        public async Task SendBestQuote(string clientId, Guid loanProcessId, LoanQuoteResponse bestQuote)
        {
            await Clients.Client(clientId).SendAsync(ReceiveBestQuote, loanProcessId, bestQuote);
        }
    }
}