using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibraryApp.Data;

// Dipakai khusus oleh perintah `dotnet ef`. Proyek ini tidak punya HostBuilder,
// jadi tanpa factory ini EF tidak tahu cara membuat AppDbContext saat membuat migration.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(AppDbContext.ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}
