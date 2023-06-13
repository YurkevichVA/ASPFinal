using Microsoft.EntityFrameworkCore;

namespace ASPFinal.Data
{
    public class DataContext: DbContext
    {
        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.Item> Items { get; set; }
        public DbSet<Entity.EmailConfirmToken> EmailConfirmTokens { get; set; }
        public DbSet<Entity.Transaction> Transactions { get; set; }
        public DataContext(DbContextOptions options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entity.Transaction>()
                .HasKey(
                    nameof(Entity.Transaction.ItemId),         // Встановлення композитного ключа
                    nameof(Entity.Transaction.UserId));


            modelBuilder.Entity<Entity.User>()
                .HasMany(u => u.Transactions)
                .WithOne()
                .HasForeignKey(t => t.ItemId);
        }
    }
}
