using LibraryApp.Domain.Entities;
using LibraryApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.UI;

public class BookMenu
{
    private readonly IServiceScopeFactory _scopeFactory;

    public BookMenu(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Buku ===");
            Console.WriteLine("1. Daftar buku");
            Console.WriteLine("2. Cari buku berdasarkan judul");
            Console.WriteLine("3. Detail buku");
            Console.WriteLine("4. Tambah buku");
            Console.WriteLine("5. Ubah buku");
            Console.WriteLine("6. Hapus buku");
            Console.WriteLine("0. Kembali");

            switch (ConsoleInput.ReadInt("Pilih: ", min: 0, max: 6))
            {
                case 1: await ListAsync(); break;
                case 2: await SearchAsync(); break;
                case 3: await DetailAsync(); break;
                case 4: await CreateAsync(); break;
                case 5: await UpdateAsync(); break;
                case 6: await DeleteAsync(); break;
                case 0: return;
            }
        }
    }

    private async Task ListAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        await PrintListAsync(books);
    }

    private async Task SearchAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        var keyword = ConsoleInput.ReadRequiredString("Kata kunci judul: ");
        var result = await books.SearchByTitleAsync(keyword);
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Error);
            return;
        }

        if (result.Value.Count == 0)
        {
            Console.WriteLine($"Tidak ada buku dengan judul yang mengandung \"{keyword}\".");
            return;
        }

        PrintTable(result.Value);
    }

    private async Task DetailAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        if (!await PrintListAsync(books))
            return;

        var id = ConsoleInput.ReadInt("Id buku: ", min: 1);
        var result = await books.GetByIdAsync(id);
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Error);
            return;
        }

        var book = result.Value;
        Console.WriteLine();
        Console.WriteLine($"Id            : {book.Id}");
        Console.WriteLine($"Judul         : {book.Title}");
        Console.WriteLine($"ISBN          : {book.Isbn}");
        Console.WriteLine($"Kategori      : {book.Category.Name}");
        Console.WriteLine($"Stok          : {book.Stock}");
        Console.WriteLine($"Tahun terbit  : {book.PublishedYear}");
        Console.WriteLine("Penulis       :");
        foreach (var bookAuthor in book.BookAuthors.OrderByDescending(ba => ba.IsPrimary).ThenBy(ba => ba.Author.Name))
            Console.WriteLine($"  - {bookAuthor.Author.Name}{(bookAuthor.IsPrimary ? " (utama)" : "")}");
    }

    private async Task CreateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        var input = await ReadBookInputAsync(scope.ServiceProvider, current: null);
        if (input is null)
            return;

        var result = await books.CreateAsync(input);

        Console.WriteLine(result.IsSuccess
            ? $"Buku berhasil ditambahkan (Id: {result.Value.Id})."
            : result.Error);
    }

    private async Task UpdateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        if (!await PrintListAsync(books))
            return;

        var id = ConsoleInput.ReadInt("Id buku yang diubah: ", min: 1);
        var current = await books.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        Console.WriteLine("Tekan Enter tanpa mengetik untuk mempertahankan nilai di dalam [ ].");
        var input = await ReadBookInputAsync(scope.ServiceProvider, current.Value);
        if (input is null)
            return;

        var result = await books.UpdateAsync(id, input);

        Console.WriteLine(result.IsSuccess ? "Buku berhasil diubah." : result.Error);
    }

    private async Task DeleteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var books = scope.ServiceProvider.GetRequiredService<IBookService>();

        if (!await PrintListAsync(books))
            return;

        var id = ConsoleInput.ReadInt("Id buku yang dihapus: ", min: 1);
        var current = await books.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        if (!ConsoleInput.Confirm($"Hapus buku \"{current.Value.Title}\""))
        {
            Console.WriteLine("Penghapusan dibatalkan.");
            return;
        }

        var result = await books.DeleteAsync(id);

        Console.WriteLine(result.IsSuccess ? "Buku berhasil dihapus." : result.Error);
    }

    // Dipakai bersama oleh Tambah dan Ubah. Saat Ubah, `current` berisi data lama yang dijadikan nilai bawaan.
    // Mengembalikan null jika belum ada kategori atau penulis yang bisa dipilih.
    private static async Task<BookInput?> ReadBookInputAsync(IServiceProvider services, Book? current)
    {
        var categories = await services.GetRequiredService<ICategoryService>().ListAsync();
        if (!categories.IsSuccess || categories.Value.Count == 0)
        {
            Console.WriteLine("Belum ada kategori. Tambahkan kategori terlebih dahulu lewat menu Kategori.");
            return null;
        }

        var authors = await services.GetRequiredService<IAuthorService>().ListAsync();
        if (!authors.IsSuccess || authors.Value.Count == 0)
        {
            Console.WriteLine("Belum ada penulis. Tambahkan penulis terlebih dahulu lewat menu Penulis.");
            return null;
        }

        var title = ConsoleInput.ReadRequiredString("Judul ", defaultValue: current?.Title);
        var isbn = ConsoleInput.ReadRequiredString("ISBN ", defaultValue: current?.Isbn);
        var stock = ConsoleInput.ReadInt("Stok ", min: 0, defaultValue: current?.Stock);
        var year = ConsoleInput.ReadInt("Tahun terbit ", min: 1000, max: DateTime.UtcNow.Year, defaultValue: current?.PublishedYear);

        Console.WriteLine();
        Console.WriteLine("Kategori:");
        foreach (var category in categories.Value)
            Console.WriteLine($"  {category.Id.ToString().PadRight(5)}{category.Name}");
        var categoryId = ConsoleInput.ReadInt("Id kategori ", min: 1, defaultValue: current?.CategoryId);

        Console.WriteLine();
        Console.WriteLine("Penulis:");
        foreach (var author in authors.Value)
            Console.WriteLine($"  {author.Id.ToString().PadRight(5)}{author.Name}");
        var authorIds = ConsoleInput.ReadIntList(
            "Id penulis (pisahkan dengan koma) ",
            current?.BookAuthors.Select(ba => ba.AuthorId).ToList());

        // Hanya satu yang boleh menjadi penulis utama, dan harus salah satu dari penulis yang dipilih
        var primaryId = authorIds[0];
        if (authorIds.Count > 1)
        {
            var currentPrimary = current?.BookAuthors.FirstOrDefault(ba => ba.IsPrimary)?.AuthorId;
            var defaultPrimary = currentPrimary is not null && authorIds.Contains(currentPrimary.Value)
                ? currentPrimary
                : null;

            while (true)
            {
                primaryId = ConsoleInput.ReadInt("Id penulis utama ", min: 1, defaultValue: defaultPrimary);
                if (authorIds.Contains(primaryId))
                    break;

                Console.WriteLine("Penulis utama harus salah satu dari penulis yang dipilih.");
            }
        }

        return new BookInput(title, isbn, stock, year, categoryId, authorIds, primaryId);
    }

    // Mengembalikan false jika tidak ada yang bisa ditampilkan, supaya aksi lanjutan tidak meminta Id yang pasti tidak ada
    private static async Task<bool> PrintListAsync(IBookService books)
    {
        var result = await books.ListAsync();
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Error);
            return false;
        }

        if (result.Value.Count == 0)
        {
            Console.WriteLine("Belum ada buku.");
            return false;
        }

        PrintTable(result.Value);
        return true;
    }

    private static void PrintTable(List<Book> books)
    {
        Console.WriteLine($"{Fit("Id", 5)} {Fit("Judul", 30)} {Fit("ISBN", 15)} {Fit("Kategori", 14)} {Fit("Stok", 5)} {Fit("Tahun", 6)} Penulis");
        Console.WriteLine(new string('-', 100));

        foreach (var book in books)
        {
            var authors = string.Join(", ", book.BookAuthors
                .OrderByDescending(ba => ba.IsPrimary)
                .ThenBy(ba => ba.Author.Name)
                .Select(ba => ba.IsPrimary ? $"{ba.Author.Name}*" : ba.Author.Name));

            Console.WriteLine(
                $"{Fit(book.Id.ToString(), 5)} {Fit(book.Title, 30)} {Fit(book.Isbn, 15)} " +
                $"{Fit(book.Category.Name, 14)} {Fit(book.Stock.ToString(), 5)} {Fit(book.PublishedYear.ToString(), 6)} {authors}");
        }

        Console.WriteLine("* = penulis utama");
    }

    // Memotong teks yang kepanjangan (diakhiri "..") atau menambah spasi di kanan, sehingga selalu selebar `width`
    private static string Fit(string text, int width)
    {
        return text.Length > width
            ? text[..(width - 2)] + ".."
            : text.PadRight(width);
    }
}
