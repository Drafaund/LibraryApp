namespace LibraryApp.Services;

// Data masukan untuk tambah/ubah buku. Satu PrimaryAuthorId dijamin tepat satu penulis utama
// (dibanding daftar IsPrimary yang bisa berisi nol atau banyak penulis utama)
public record BookInput(
    string? Title,
    string? Isbn,
    int Stock,
    int PublishedYear,
    int CategoryId,
    IReadOnlyList<int> AuthorIds,
    int PrimaryAuthorId);
