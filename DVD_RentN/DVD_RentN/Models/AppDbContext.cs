using Microsoft.EntityFrameworkCore;

namespace DVD_Rent.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Film> Film { get; set; }
        public DbSet<Kiralama> Kiralama { get; set; }
        public DbSet<Musteri> Musteri { get; set; }
    }
}
