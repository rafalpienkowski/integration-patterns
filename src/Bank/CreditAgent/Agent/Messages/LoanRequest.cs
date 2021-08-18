namespace CreditAgent.Agent.Messages
{
    public class LoanRequest
    {
        public string UserName { get; set; }
        public string ClientId { get; set; }
        public int LoanTerm { get; set; }
        public int LoanAmount { get; set; }
    }
}