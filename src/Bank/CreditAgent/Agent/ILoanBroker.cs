using CreditAgent.Agent.Messages;

namespace CreditAgent.Agent
{
    public interface ILoanBroker
    {
        public void SendLoanRequest(LoanRequest loanRequest);
        public void PublishBestLoan(string clientId);
    }
}