using LibraryApp.Common;
using LibraryApp.Data;
using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApp.Services;

public class BookService : IBookService
{
    private const int MaxTitleLength = 200;
    private const int MaxIsbnLength = 20;
    private const int MinPublishedYear = 1000;

    private readonly AppDbContext _db;

    public BookService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Result<List<Book>>> ListAsync()
    {
        var books = await BooksWithDetails()
            .OrderBy(b => b.Title)
            .ToListAsync();

        return Result<List<Book>>.Ok(books);
    }

    public async Task<Result<List<Book>>> SearchByTitleAsync(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await ListAsync();

        // Dibandingkan dalam huruf kecil karena Contains di SQLite membedakan huruf besar/kecil
        var lowered = keyword.Trim().ToLower();
        var books = await BooksWithDetails()
            .Where(b => b.Title.ToLower().Contains(lowered))
            .OrderBy(b => b.Title)
            .ToListAsync();

        return Result<List<Book>>.Ok(books);
    }

    public async Task<Result<Book>> GetByIdAsync(int id)
    {
        var book = await BooksWithDetails()
            .FirstOrDefaultAsync(b => b.Id == id);

        return book is null
            ? Result<Book>.Fail("Buku tidak ditemukan.")
            : Result<Book>.Ok(book);
    }

    public async Task<Result<Book>> CreateAsync(BookInput input)
    {
        var error = await ValidateAsync(input, excludeBookId: null);
        if (error is not null)
            return Result<Book>.Fail(error);

        var book = new Book
        {
            Title = input.Title!.Trim(),
            Isbn = input.Isbn!.Trim(),
            Stock = input.Stock,
            PublishedYear = input.PublishedYear,
            CategoryId = input.CategoryId,
            BookAuthors = input.AuthorIds
                .Select(authorId => new BookAuthor
                {
                    AuthorId = authorId,
                    IsPrimary = authorId == input.PrimaryAuthorId
                })
                .ToList()
        };

        // Buku dan baris pivot-nya disimpan dalam satu SaveChanges, jadi keduanya berhasil atau gagal bersama
        _db.Books.Add(book);
        await _db.SaveChangesAsync();

        // Dimuat ulang agar Category dan Author terisi untuk ditampilkan
        return await GetByIdAsync(book.Id);
    }

    public async Task<Result> UpdateAsync(int id, BookInput input)
    {
        // Dimuat beserta BookAuthors dan tanpa AsNoTracking: relasi penulis akan diubah dan harus dilacak EF
        var book = await _db.Books
            .Include(b => b.BookAuthors)
            .FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
            return Result.Fail("Buku tidak ditemukan.");

        var error = await ValidateAsync(input, excludeBookId: id);
        if (error is not null)
            return Result.Fail(error);

        book.Title = input.Title!.Trim();
        book.Isbn = input.Isbn!.Trim();
        book.Stock = input.Stock;
        book.PublishedYear = input.PublishedYear;
        book.CategoryId = input.CategoryId;

        // Hasil akhirnya sama dengan "hapus semua penulis lama, tambah yang baru", tetapi dilakukan sebagai selisih:
        // menghapus lalu menambah baris dengan key (BookId, AuthorId) yang sama dalam satu SaveChanges membuat EF error
        var newAuthorIds = input.AuthorIds.ToHashSet();

        var removed = book.BookAuthors.Where(ba => !newAuthorIds.Contains(ba.AuthorId)).ToList();
        _db.BookAuthors.RemoveRange(removed);

        foreach (var kept in book.BookAuthors.Except(removed))
            kept.IsPrimary = kept.AuthorId == input.PrimaryAuthorId;

        var existingAuthorIds = book.BookAuthors.Select(ba => ba.AuthorId).ToHashSet();
        foreach (var authorId in newAuthorIds.Where(a => !existingAuthorIds.Contains(a)))
        {
            book.BookAuthors.Add(new BookAuthor
            {
                AuthorId = authorId,
                IsPrimary = authorId == input.PrimaryAuthorId
            });
        }

        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book is null)
            return Result.Fail("Buku tidak ditemukan.");

        // Baris pivot BookAuthors ikut terhapus oleh Cascade di database
        _db.Books.Remove(book);
        await _db.SaveChangesAsync();

        return Result.Ok();
    }

    // Mengembalikan pesan error, atau null jika input valid.
    // excludeBookId dipakai saat update agar ISBN milik buku itu sendiri tidak dianggap kembar
    private async Task<string?> ValidateAsync(BookInput input, int? excludeBookId)
    {
        if (string.IsNullOrWhiteSpace(input.Title))
            return "Judul buku tidak boleh kosong.";

        if (input.Title.Trim().Length > MaxTitleLength)
            return $"Judul buku maksimal {MaxTitleLength} karakter.";

        if (string.IsNullOrWhiteSpace(input.Isbn))
            return "ISBN tidak boleh kosong.";

        if (input.Isbn.Trim().Length > MaxIsbnLength)
            return $"ISBN maksimal {MaxIsbnLength} karakter.";

        if (input.Stock < 0)
            return "Stok tidak boleh negatif.";

        var currentYear = DateTime.UtcNow.Year;
        if (input.PublishedYear < MinPublishedYear || input.PublishedYear > currentYear)
            return $"Tahun terbit harus antara {MinPublishedYear} dan {currentYear}.";

        // Dibandingkan dalam huruf kecil agar ISBN berakhiran "X" tidak lolos hanya karena beda huruf besar/kecil
        var isbn = input.Isbn.Trim().ToLower();
        if (await _db.Books.AnyAsync(b => b.Isbn.ToLower() == isbn && b.Id != excludeBookId))
            return $"ISBN \"{input.Isbn.Trim()}\" sudah dipakai buku lain.";

        if (!await _db.Categories.AnyAsync(c => c.Id == input.CategoryId))
            return "Kategori tidak ditemukan.";

        var authorIds = input.AuthorIds.ToList();
        if (authorIds.Count == 0)
            return "Buku harus punya minimal satu penulis.";

        if (authorIds.Distinct().Count() != authorIds.Count)
            return "Penulis yang sama tidak boleh dipilih dua kali.";

        if (!authorIds.Contains(input.PrimaryAuthorId))
            return "Penulis utama harus termasuk dalam daftar penulis buku.";

        var existingAuthorCount = await _db.Authors.CountAsync(a => authorIds.Contains(a.Id));
        if (existingAuthorCount != authorIds.Count)
            return "Ada penulis yang tidak ditemukan.";

        return null;
    }

    // Include memuat Category dan Author dalam satu query, supaya tidak ada query tambahan per buku (masalah N+1)
    private IQueryable<Book> BooksWithDetails()
    {
        return _db.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .Include(b => b.BookAuthors)
                .ThenInclude(ba => ba.Author);
    }
}
