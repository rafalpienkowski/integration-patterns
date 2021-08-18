using System;

namespace CreditAgent.Agent.Messages
{
    public class CreditBureauResponse
    {
        public Guid LoanProcessId { get; set; }
        public string UserName { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLenght { get; set; }
    }
}