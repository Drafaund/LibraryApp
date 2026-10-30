using LibraryApp.Data;
using LibraryApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

// AddDbContext mendaftarkan AppDbContext sebagai Scoped: satu instance per scope
services.AddDbContext<AppDbContext>(options => options.UseSqlite(AppDbContext.ConnectionString));
services.AddScoped<ICategoryService, CategoryService>();
services.AddScoped<IAuthorService, AuthorService>();
services.AddScoped<IBookService, BookService>();

await using var provider = services.BuildServiceProvider();

// Scope sendiri untuk startup, agar DbContext-nya dibuang setelah migrate dan seed selesai
using (var scope = provider.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

Console.WriteLine("Database siap.");
