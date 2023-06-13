namespace ASPFinal.Data.Entity
{
    public class Transaction
    {
        public Guid UserId { get; set; }
        public Guid ItemId { get; set; }
        public DateTime TransactionDate { get; set; }
    }
}
