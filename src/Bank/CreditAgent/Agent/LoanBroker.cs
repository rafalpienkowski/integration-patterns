using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CreditAgent.Agent.Messages;
using IntegrationFramework;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CreditAgent.Agent
{
    public class LoanBroker : ILoanBroker
    {
        private const string CreditRequestQueue = "credit-request";
        private const string CreditResponseQueue = "credit-response";
        private const string LoanQuoteRequestQueue = "loan-quote-request";
        private const string LoanQuoteResponseQueue = "loan-quote-response";
        private readonly ILogger<LoanBroker> _logger;
        private readonly IMessageBroker _messageBroker;
        private readonly LoanHub _loanHub;

        private static readonly IDictionary<Guid, OngoingLoanRequest> OngoingLoanProcesses =
            new ConcurrentDictionary<Guid, OngoingLoanRequest>();

        public LoanBroker(ILogger<LoanBroker> logger, IMessageBroker messageBroker, LoanHub loanHub)
        {
            _logger = logger;
            _messageBroker = messageBroker;
            _loanHub = loanHub;

            _messageBroker.OnResponse<CreditBureauResponse>(CreditResponseQueue, OnCreditBureauResponse);
            _messageBroker.OnResponse<LoanQuoteResponse>(LoanQuoteResponseQueue, OnBankResponse);
        }

        public void SendLoanRequest(LoanRequest loanRequest)
        {
            _logger.LogInformation($"New loan request from {loanRequest.UserName}");
            
            var loanProcessId = CreateNewLoanProcess(loanRequest);

            GetCreditScore(new CreditBureauRequest
            {
                LoanProcessId = loanProcessId,
                UserName = loanRequest.UserName
            });
        }

        public void PublishBestLoan(string clientId)
        {
            var ongoingLoanProcess =
                OngoingLoanProcesses.Values.FirstOrDefault(lp => lp.ClientId == clientId && !lp.IsCompleted);

            if (ongoingLoanProcess == null)
            {
                return;
            }
            
            ongoingLoanProcess.Finish();
            
            Task.Run(async () => await _loanHub.SendBestQuote(ongoingLoanProcess.ClientId, ongoingLoanProcess.Id, ongoingLoanProcess.BestQuote));
            
            _logger.LogInformation($"Best bank response: {JsonConvert.SerializeObject(ongoingLoanProcess.BestQuote)}");
        }

        private static Guid CreateNewLoanProcess(LoanRequest loanRequest)
        {
            var loanProcessId = Guid.NewGuid();
            OngoingLoanProcesses.Add(loanProcessId, new OngoingLoanRequest
            {
                Id = loanProcessId,
                UserName = loanRequest.UserName,
                ClientId = loanRequest.ClientId,
                LoanAmount = loanRequest.LoanAmount,
                LoanTerm = loanRequest.LoanTerm,
                BankQuotes = new BankQuoteAggregate(3)
            });
            return loanProcessId;
        }

        private void GetCreditScore(CreditBureauRequest request)
        {
            _messageBroker.SendMessage(CreditRequestQueue, CreditResponseQueue, request.LoanProcessId.ToString(), request);
        }

        private void OnCreditBureauResponse(CreditBureauResponse response, MessageProperties messageProperties)
        {
            var correlationId = ExtractCorrelationId(messageProperties.CorrelationId);
            response.LoanProcessId = correlationId;

            var ongoingLoanProcess = OngoingLoanProcesses[correlationId];
            ongoingLoanProcess.CreditScore = response.CreditScore;
            ongoingLoanProcess.HistoryLenght = response.HistoryLenght;

            Task.Run(async () => await _loanHub.SendCreditScore(ongoingLoanProcess.ClientId, ongoingLoanProcess.Id, response));
            
            SendRequestToBanks(new LoanQuoteRequest
            {
                CreditScore = ongoingLoanProcess.CreditScore,
                CreditHistoryLenght = ongoingLoanProcess.HistoryLenght,
                LoanAmount = ongoingLoanProcess.LoanAmount,
                LoanDuration = ongoingLoanProcess.LoanTerm,
                UserName = ongoingLoanProcess.UserName
            }, ongoingLoanProcess.Id);
        }

        private Guid ExtractCorrelationId(string correlationString)
        {
            var correlationIdParsed = Guid.TryParse(correlationString, out var correlationId);
            if (!correlationIdParsed || !OngoingLoanProcesses.ContainsKey(correlationId))
            {
                _logger.LogWarning($"Ongoing loan process found: {correlationString}");
            }

            return correlationId;
        }

        private void OnBankResponse(LoanQuoteResponse response, MessageProperties messageProperties)
        {
            var correlationId = ExtractCorrelationId(messageProperties.CorrelationId);
            
            var ongoingLoanProcess = OngoingLoanProcesses[correlationId];
            
            Task.Run(async () => await _loanHub.SendBankQuote(ongoingLoanProcess.ClientId, ongoingLoanProcess.Id, response));
            _logger.LogInformation($"Bank response: {JsonConvert.SerializeObject(response)}");

            if (ongoingLoanProcess.IsCompleted) return;
            
            ongoingLoanProcess.BankQuotes.AddResponse(response);

            if (!ongoingLoanProcess.IsCompleted) return;
            
            Task.Run(async () => await _loanHub.SendBestQuote(ongoingLoanProcess.ClientId,
                ongoingLoanProcess.Id, ongoingLoanProcess.BestQuote));

            _logger.LogInformation(
                $"Best bank response: {JsonConvert.SerializeObject(ongoingLoanProcess.BestQuote)}");
        }

        private void SendRequestToBanks(LoanQuoteRequest request, Guid correlationId)
        {
            _messageBroker.Publish(LoanQuoteRequestQueue, correlationId.ToString(), request);
        }
    }

    public class BankQuoteAggregate
    {
        private readonly List<LoanQuoteResponse> _responses = new List<LoanQuoteResponse>();
        private readonly int _expectedMessages;
        private bool _isCompleted;
        private double _bestInterest = Double.MaxValue;
        public LoanQuoteResponse BestQuoteResponse { get; private set; } = null;

        public BankQuoteAggregate(int expectedMessages)
        {
            _expectedMessages = expectedMessages;
        }

        public bool IsCompleted
        {
            get
            {
                if (_isCompleted)
                {
                    return true;
                }
                
                return _responses.Count >= _expectedMessages;
            }
        }

        public void Finish()
        {
            _isCompleted = true;
        }

        public void AddResponse(LoanQuoteResponse response)
        {
            if (response.LoanInterest < _bestInterest)
            {
                _bestInterest = response.LoanInterest;
                BestQuoteResponse = response;
            }
            
            _responses.Add(response);
        }
    }
}