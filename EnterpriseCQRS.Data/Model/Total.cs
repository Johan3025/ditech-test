using System.Collections.Generic;

namespace EnterpriseCQRS.Data.Model
{
    public class Total
    {
        public List<TotalTransactionList> Totalesporsku { get; set; }
        public decimal TotalSkus { get; set; }
    }
}
