using LibraryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApp.Data.Configurations;

// Satu class konfigurasi per entity; otomatis dimuat oleh ApplyConfigurationsFromAssembly di AppDbContext
public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        // Kolom Name: NOT NULL dan maksimal 100 karakter
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Dua kategori tidak boleh punya nama yang sama (dijaga database lewat unique index)
        builder.HasIndex(c => c.Name)
            .IsUnique();
    }
}
