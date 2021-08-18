using System;

namespace CreditAgent.Agent.Messages
{
    public class CreditBureauRequest
    {
        public string UserName { get; set; }
        public Guid LoanProcessId { get; set; }
    }
}