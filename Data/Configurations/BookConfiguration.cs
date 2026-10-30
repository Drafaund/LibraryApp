using LibraryApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(20);

        // Dua buku tidak boleh punya ISBN yang sama
        builder.HasIndex(b => b.Isbn)
            .IsUnique();

        // Relasi 1:N: satu Category punya banyak Book, setiap Book wajib punya satu Category.
        // Restrict = Category tidak bisa dihapus selama masih punya Book (database menolak)
        builder.HasOne(b => b.Category)
            .WithMany(c => c.Books)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
