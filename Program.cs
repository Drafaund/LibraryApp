using LibraryApp.Data;
using LibraryApp.Services;
using LibraryApp.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// AddDbContext mendaftarkan AppDbContext sebagai Scoped: satu instance per scope
services.AddDbContext<AppDbContext>(options => options.UseSqlite(AppDbContext.ConnectionString));
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IAuthorService, AuthorService>();
services.AddScoped<IBookService, BookService>();

// Menu hanya menyimpan IServiceScopeFactory (tidak ada DbContext), jadi aman didaftarkan Singleton
services.AddSingleton<CategoryMenu>();
services.AddSingleton<AuthorMenu>();
services.AddSingleton<BookMenu>();
services.AddSingleton<MainMenu>();

await using var provider = services.BuildServiceProvider(); //sifatnya singleton

// Provider itu sifatnya singleton, tapi DbContext itu sifatnya scoped, 
// jadi harus dibuat scope baru untuk setiap penggunaan DbContext.
// Scope sendiri untuk startup, agar DbContext-nya dibuang setelah migrate dan seed selesai
using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

try
{
    await provider.GetRequiredService<MainMenu>().RunAsync();
    Console.WriteLine("Sampai jumpa."); 
}
catch (EndOfStreamException)
{
    // Input ditutup (Ctrl+Z / pipe habis): keluar dengan rapi alih-alih menampilkan stack trace
    Console.WriteLine();
    Console.WriteLine("Input ditutup, aplikasi berhenti.");
}
