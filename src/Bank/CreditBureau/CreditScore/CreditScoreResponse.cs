namespace CreditBureau.CreditScore
{
    public class CreditScoreResponse
    {
        public string UserName { get; set; }
        public int CreditScore { get; set; }
        public int HistoryLenght { get; set; }
    }
}