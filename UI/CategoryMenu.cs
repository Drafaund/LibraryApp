using LibraryApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.UI;

public class CategoryMenu
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CategoryMenu(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Kategori ===");
            Console.WriteLine("1. Daftar kategori");
            Console.WriteLine("2. Tambah kategori");
            Console.WriteLine("3. Ubah kategori");
            Console.WriteLine("4. Hapus kategori");
            Console.WriteLine("0. Kembali");

            switch (ConsoleInput.ReadInt("Pilih: ", min: 0, max: 4))
            {
                case 1: await ListAsync(); break;
                case 2: await CreateAsync(); break;
                case 3: await UpdateAsync(); break;
                case 4: await DeleteAsync(); break;
                case 0: return;
            }
        }
    }

    // Setiap aksi membuat scope DI baru, jadi DbContext-nya baru dan dibuang setelah aksi selesai
    private async Task ListAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        await PrintListAsync(service);
    }

    private async Task CreateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        var name = ConsoleInput.ReadRequiredString("Nama kategori: ");
        var result = await service.CreateAsync(name);

        Console.WriteLine(result.IsSuccess
            ? $"Kategori berhasil ditambahkan (Id: {result.Value.Id})."
            : result.Error);
    }

    private async Task UpdateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        if (!await PrintListAsync(service))
            return;

        var id = ConsoleInput.ReadInt("Id kategori yang diubah: ", min: 1);
        var current = await service.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        var name = ConsoleInput.ReadRequiredString("Nama baru ", defaultValue: current.Value.Name);
        var result = await service.UpdateAsync(id, name);

        Console.WriteLine(result.IsSuccess ? "Kategori berhasil diubah." : result.Error);
    }

    private async Task DeleteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICategoryService>();

        if (!await PrintListAsync(service))
            return;

        var id = ConsoleInput.ReadInt("Id kategori yang dihapus: ", min: 1);
        var current = await service.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        if (!ConsoleInput.Confirm($"Hapus kategori \"{current.Value.Name}\""))
        {
            Console.WriteLine("Penghapusan dibatalkan.");
            return;
        }

        var result = await service.DeleteAsync(id);

        Console.WriteLine(result.IsSuccess ? "Kategori berhasil dihapus." : result.Error);
    }

    // Mengembalikan false jika tidak ada yang bisa ditampilkan, supaya Ubah/Hapus tidak meminta Id yang pasti tidak ada
    private static async Task<bool> PrintListAsync(ICategoryService service)
    {
        var result = await service.ListAsync();
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Error);
            return false;
        }

        if (result.Value.Count == 0)
        {
            Console.WriteLine("Belum ada kategori.");
            return false;
        }

        // PadRight menambah spasi di kanan sampai lebar tertentu, supaya kolom sejajar
        Console.WriteLine($"{"Id".PadRight(6)}Nama");
        Console.WriteLine(new string('-', 40));
        foreach (var category in result.Value)
            Console.WriteLine($"{category.Id.ToString().PadRight(6)}{category.Name}");

        return true;
    }
}
