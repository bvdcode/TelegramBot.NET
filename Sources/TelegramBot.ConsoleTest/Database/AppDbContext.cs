using Microsoft.EntityFrameworkCore;

namespace TelegramBot.ConsoleTest.Database
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<SomeDataRow> SomeDataRows { get; set; }
    }
}