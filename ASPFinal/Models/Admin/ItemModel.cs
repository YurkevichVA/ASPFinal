namespace ASPFinal.Models.Admin
{
    public class ItemModel
    {
        public string Name { get; set; } = null!;
        public IFormFile Content { get; set; } = null!;
        public int Type { get; set; }
        public int CostCoins { get; set; }
    }
}
