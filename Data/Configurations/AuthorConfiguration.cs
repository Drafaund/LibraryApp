using LibraryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Data.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        // Nama penulis boleh kembar (dua orang bisa bernama sama), jadi tidak ada unique index di sini
        builder.Property(a => a.Name) //a itu alias dari atribut tabel
            .IsRequired()
            .HasMaxLength(150);
    }
}
