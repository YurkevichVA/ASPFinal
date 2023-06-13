namespace ASPFinal.Models.Shop
{
    public class ItemModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string Content { get; set; } = null!;
        public int CostCoins { get; set; }
        public string Type { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}
