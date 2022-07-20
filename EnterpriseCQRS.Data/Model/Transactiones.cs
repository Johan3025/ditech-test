namespace EnterpriseCQRS.Data.Model
{
    public class Transactions : Transaction
    {
        public decimal Convertion { get; set; }
        public string CurrencyChange { get; set; }
    }
}
