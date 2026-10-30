namespace LibraryApp.UI;

public class MainMenu
{
    private readonly CategoryMenu _categoryMenu;
    private readonly AuthorMenu _authorMenu;
    private readonly BookMenu _bookMenu;

    public MainMenu(CategoryMenu categoryMenu, AuthorMenu authorMenu, BookMenu bookMenu)
    {
        _categoryMenu = categoryMenu;
        _authorMenu = authorMenu;
        _bookMenu = bookMenu;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== LibraryApp ===");
            Console.WriteLine("1. Kategori");
            Console.WriteLine("2. Penulis");
            Console.WriteLine("3. Buku");
            Console.WriteLine("0. Keluar");

            switch (ConsoleInput.ReadInt("Pilih: ", min: 0, max: 3))
            {
                case 1: await _categoryMenu.RunAsync(); break;
                case 2: await _authorMenu.RunAsync(); break;
                case 3: await _bookMenu.RunAsync(); break;
                case 0: return;
            }
        }
    }
}
