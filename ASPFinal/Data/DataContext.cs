using Microsoft.EntityFrameworkCore;

namespace ASPFinal.Data
{
    public class DataContext: DbContext
    {
        public DbSet<Entity.User> Users { get; set; }
        public DbSet<Entity.Item> Items { get; set; }
        public DataContext(DbContextOptions options) : base(options) { }
    }
}
