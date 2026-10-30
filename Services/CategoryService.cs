using LibraryApp.Common;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public class CategoryService : ICategoryService
{
    private const int MaxNameLength = 100;

    private readonly AppDbContext _db;

    public CategoryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<Category>>> ListAsync()
    {
        // AsNoTracking: data hanya dibaca, jadi EF tidak perlu melacak perubahannya
        var categories = await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Result<List<Category>>.Ok(categories);
    }

    public async Task<Result<Category>> GetByIdAsync(int id)
    {
        var category = await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return category is null
            ? Result<Category>.Fail("Kategori tidak ditemukan.")
            : Result<Category>.Ok(category);
    }

    public async Task<Result<Category>> CreateAsync(string? name)
    {
        var validation = ValidateName(name);
        if (validation is not null)
            return Result<Category>.Fail(validation);

        var trimmed = name!.Trim();
        if (await NameExistsAsync(trimmed, excludeId: null))
            return Result<Category>.Fail($"Kategori \"{trimmed}\" sudah ada.");

        var category = new Category { Name = trimmed };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Result<Category>.Ok(category);
    }

    public async Task<Result> UpdateAsync(int id, string? name)
    {
        var validation = ValidateName(name);
        if (validation is not null)
            return Result.Fail(validation);

        // Tanpa AsNoTracking: objek ini akan diubah, jadi EF harus melacaknya agar SaveChanges menyimpan perubahan
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return Result.Fail("Kategori tidak ditemukan.");

        var trimmed = name!.Trim();
        if (await NameExistsAsync(trimmed, excludeId: id))
            return Result.Fail($"Kategori \"{trimmed}\" sudah ada.");

        category.Name = trimmed;
        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category is null)
            return Result.Fail("Kategori tidak ditemukan.");

        // Dicek di sini agar user dapat pesan yang jelas, bukan exception foreign key dari database
        var bookCount = await _db.Books.CountAsync(b => b.CategoryId == id);
        if (bookCount > 0)
            return Result.Fail($"Kategori masih dipakai oleh {bookCount} buku, hapus atau pindahkan bukunya terlebih dahulu.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    private static string? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Nama kategori tidak boleh kosong.";

        if (name.Trim().Length > MaxNameLength)
            return $"Nama kategori maksimal {MaxNameLength} karakter.";

        return null;
    }

    // Dibandingkan dalam huruf kecil karena unique index SQLite membedakan huruf besar/kecil
    private async Task<bool> NameExistsAsync(string name, int? excludeId)
    {
        var lowered = name.ToLower();
        return await _db.Categories.AnyAsync(c => c.Name.ToLower() == lowered && c.Id != excludeId);
    }
}
