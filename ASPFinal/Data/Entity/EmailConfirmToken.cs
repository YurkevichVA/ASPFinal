namespace ASPFinal.Data.Entity
{
    public class EmailConfirmToken
    {
        public Guid     Id { get; set; }
        public Guid     UserId { get; set; }
        public string   UserEmail { get; set; } = null!;
        public DateTime Moment { get; set; }
        public int      Used { get; set; } = 0;
    }
}
