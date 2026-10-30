using LibraryApp.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApp.UI;

public class AuthorMenu
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AuthorMenu(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Penulis ===");
            Console.WriteLine("1. Daftar penulis");
            Console.WriteLine("2. Tambah penulis");
            Console.WriteLine("3. Ubah penulis");
            Console.WriteLine("4. Hapus penulis");
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

    private async Task ListAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthorService>();

        await PrintListAsync(service);
    }

    private async Task CreateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthorService>();

        var name = ConsoleInput.ReadRequiredString("Nama penulis: ");
        var result = await service.CreateAsync(name);

        Console.WriteLine(result.IsSuccess
            ? $"Penulis berhasil ditambahkan (Id: {result.Value.Id})."
            : result.Error);
    }

    private async Task UpdateAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthorService>();

        if (!await PrintListAsync(service))
            return;

        var id = ConsoleInput.ReadInt("Id penulis yang diubah: ", min: 1);
        var current = await service.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        var name = ConsoleInput.ReadRequiredString("Nama baru ", defaultValue: current.Value.Name);
        var result = await service.UpdateAsync(id, name);

        Console.WriteLine(result.IsSuccess ? "Penulis berhasil diubah." : result.Error);
    }

    private async Task DeleteAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IAuthorService>();

        if (!await PrintListAsync(service))
            return;

        var id = ConsoleInput.ReadInt("Id penulis yang dihapus: ", min: 1);
        var current = await service.GetByIdAsync(id);
        if (!current.IsSuccess)
        {
            Console.WriteLine(current.Error);
            return;
        }

        if (!ConsoleInput.Confirm($"Hapus penulis \"{current.Value.Name}\""))
        {
            Console.WriteLine("Penghapusan dibatalkan.");
            return;
        }

        var result = await service.DeleteAsync(id);

        Console.WriteLine(result.IsSuccess ? "Penulis berhasil dihapus." : result.Error);
    }

    private static async Task<bool> PrintListAsync(IAuthorService service)
    {
        var result = await service.ListAsync();
        if (!result.IsSuccess)
        {
            Console.WriteLine(result.Error);
            return false;
        }

        if (result.Value.Count == 0)
        {
            Console.WriteLine("Belum ada penulis.");
            return false;
        }

        Console.WriteLine($"{"Id".PadRight(6)}Nama");
        Console.WriteLine(new string('-', 40));
        foreach (var author in result.Value)
            Console.WriteLine($"{author.Id.ToString().PadRight(6)}{author.Name}");

        return true;
    }
}
