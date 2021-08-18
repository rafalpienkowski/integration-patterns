namespace Bank.Loans
{
    public class LoanQuoteRequest
    {
        public string UserName { get; set; }
        public int LoanDuration { get; set; }
        public int LoanAmount { get; set; }
        public int CreditHistoryLenght { get; set; }
        public int CreditScore { get; set; }
    }
}