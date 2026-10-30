using LibraryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Data.Configurations;

public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
{
    public void Configure(EntityTypeBuilder<BookAuthor> builder)
    {
        // Composite key: pasangan (BookId, AuthorId) harus unik, jadi penulis yang sama tidak bisa tercatat dua kali di satu buku
        builder.HasKey(ba => new { ba.BookId, ba.AuthorId });

        // Hapus Book -> baris pivot miliknya ikut terhapus (Cascade)
        builder.HasOne(ba => ba.Book)
            .WithMany(b => b.BookAuthors)
            .HasForeignKey(ba => ba.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // Hapus Author -> baris pivot miliknya ikut terhapus (Cascade); Service sudah menolak jika Author masih dipakai Book
        builder.HasOne(ba => ba.Author)
            .WithMany(a => a.BookAuthors)
            .HasForeignKey(ba => ba.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
