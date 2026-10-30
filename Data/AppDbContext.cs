using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Data;

public class AppDbContext : DbContext
{
    // Satu sumber connection string, dipakai Program.cs dan DesignTimeDbContextFactory agar keduanya menunjuk file DB yang sama
    public const string ConnectionString = "Data Source=library.db";

    // Menerima pengaturan (provider SQLite, connection string) dari luar, lalu meneruskannya ke DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Setiap DbSet<T> mewakili satu tabel di database dan menjadi pintu untuk query/simpan data T.
    // Categories, Authors, Books, BookAuthors = nama tabel yang akan dibuat lewat migration
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<BookAuthor> BookAuthors => Set<BookAuthor>();

    // Dipanggil EF Core sekali saat membangun model: tempat mengatur aturan tabel (key, relasi, index)
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Memuat otomatis semua class konfigurasi di Data/Configurations, jadi tidak perlu didaftarkan satu-satu
        // Dia akan mencari semua class yang mengimplementasi IEntityTypeConfiguration<T> di assembly ini
        // lalu memanggil Configure-nya
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); 

    }
}
