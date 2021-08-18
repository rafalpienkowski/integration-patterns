using System;
using CreditAgent.Agent.Messages;

namespace CreditAgent.Agent
{
    public class OngoingLoanRequest
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public int LoanTerm { get; set; }
        public int LoanAmount { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLenght { get; set; }
        public BankQuoteAggregate BankQuotes { get; set; }

        public bool IsCompleted => BankQuotes.IsCompleted;
        public void Finish() => BankQuotes.Finish();

        public LoanQuoteResponse BestQuote => IsCompleted ? BankQuotes.BestQuoteResponse : null;
    }
}