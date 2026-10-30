using LibraryApp.Common;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public class AuthorService : IAuthorService
{
    private const int MaxNameLength = 150;

    private readonly AppDbContext _db;

    public AuthorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<Author>>> ListAsync()
    {
        var authors = await _db.Authors
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .ToListAsync();

        return Result<List<Author>>.Ok(authors);
    }

    public async Task<Result<Author>> GetByIdAsync(int id)
    {
        var author = await _db.Authors
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return author is null
            ? Result<Author>.Fail("Penulis tidak ditemukan.")
            : Result<Author>.Ok(author);
    }

    public async Task<Result<Author>> CreateAsync(string? name)
    {
        var validation = ValidateName(name);
        if (validation is not null)
            return Result<Author>.Fail(validation);

        var author = new Author { Name = name!.Trim() };
        _db.Authors.Add(author);
        await _db.SaveChangesAsync();

        return Result<Author>.Ok(author);
    }

    public async Task<Result> UpdateAsync(int id, string? name)
    {
        var validation = ValidateName(name);
        if (validation is not null)
            return Result.Fail(validation);

        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (author is null)
            return Result.Fail("Penulis tidak ditemukan.");

        author.Name = name!.Trim();
        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var author = await _db.Authors.FirstOrDefaultAsync(a => a.Id == id);
        if (author is null)
            return Result.Fail("Penulis tidak ditemukan.");

        // Pivot memakai Cascade, jadi tanpa pengecekan ini penulis akan terhapus diam-diam dari buku-bukunya
        var bookCount = await _db.BookAuthors.CountAsync(ba => ba.AuthorId == id);
        if (bookCount > 0)
            return Result.Fail($"Penulis masih dipakai oleh {bookCount} buku, ubah penulis bukunya terlebih dahulu.");

        _db.Authors.Remove(author);
        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    private static string? ValidateName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "Nama penulis tidak boleh kosong.";

        if (name.Trim().Length > MaxNameLength)
            return $"Nama penulis maksimal {MaxNameLength} karakter.";

        return null;
    }
}
