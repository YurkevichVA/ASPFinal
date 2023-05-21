namespace ASPFinal.Data.Entity
{
    public class Item
    {
        public Guid         Id          { get; set; }
        public string       Name        { get; set; } = null!;
        public string?      Content     { get; set; } = null!;
        public int          CostCoins   { get; set; }
        public int          Type        { get; set; }
        public DateTime?    DeleteDt    { get; set; }
    }
}
