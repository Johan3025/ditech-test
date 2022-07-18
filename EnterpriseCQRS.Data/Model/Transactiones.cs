namespace EnterpriseCQRS.Data.Model
{
    public class Transactiones : Transaction
    {
        public decimal Convertion { get; set; }
        public string CurrencyChange { get; set; }
    }
}
