# LibraryApp

LibraryApp is a console application for managing a library's book data, built with C# (.NET 8) and EF Core (SQLite) as the "Create CRUD Using EF Core" assignment. It demonstrates CRUD operations for Category, Author, and Book, a one-to-many relationship (Category–Book), a many-to-many relationship with an explicit join entity (Book–Author), and migrations.

## Tech stack

- .NET 8 (C#, nullable reference types enabled)
- EF Core 8 with the SQLite provider
- `Microsoft.Extensions.DependencyInjection` for dependency injection

## Prerequisites

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0) (the version is pinned via `global.json`)
- (Optional, only needed to create new migrations) the EF tool:
  ```bash
  dotnet tool install --global dotnet-ef --version 8.*
  ```

## Getting started

Run from the project root folder (the one containing `LibraryApp.csproj`):

```bash
dotnet build
dotnet run
```

On first run, the application automatically:

1. creates the `library.db` database in the working directory,
2. applies all migrations (creates the tables),
3. seeds sample data if the database is empty (2 categories, 3 authors, 2 books),
4. shows the main menu.

There is no login. All features are available immediately.

To start over, delete `library.db` (along with `library.db-shm` and `library.db-wal` if present) and run `dotnet run` again.

The menu text and messages shown to the user are in Indonesian.

## Features

| Menu | Features |
| ---- | -------- |
| Category (Kategori) | List, add, edit, delete |
| Author (Penulis) | List, add, edit, delete |
| Book (Buku) | List (table with category and authors), search by title, details, add, edit, delete |

Rules enforced by the application:

- Category names are unique (case-insensitive); a category that still has books cannot be deleted.
- An author that is still linked to a book cannot be deleted.
- ISBN is unique; `Stock >= 0`; the publication year must be between 1000 and the current year.
- Every book has one category and at least one author, with exactly one primary author.
- Editing a book can change its category and authors; deleting a book also removes its author links.
- Invalid input (text in a numeric field, empty values, out-of-range numbers) is rejected with a message and asked again, without stopping the application.

## Data model

```mermaid
erDiagram
    Categories ||--o{ Books : "has many"
    Books ||--o{ BookAuthors : "written by"
    Authors ||--o{ BookAuthors : "writes"

    Categories {
        int Id PK
        string Name UK
    }
    Authors {
        int Id PK
        string Name
    }
    Books {
        int Id PK
        string Title
        string Isbn UK
        int Stock
        int PublishedYear
        int CategoryId FK
    }
    BookAuthors {
        int BookId PK, FK
        int AuthorId PK, FK
        bool IsPrimary
    }
```

- **Category – Book (1:N):** `OnDelete(Restrict)`, so a category cannot be deleted while it still has books.
- **Book – Author (N:M):** through the explicit join entity `BookAuthors`, which has the composite key `(BookId, AuthorId)` and the extra column `IsPrimary`. The join rows use `Cascade` delete.

## Project structure

```
LibraryApp/
├─ Program.cs               Composition root: DI, migrate, seed, run the menu
├─ Common/Result.cs         Operation result (success/failure + message)
├─ Models/                  Entities: Category, Author, Book, BookAuthor
├─ Data/
│  ├─ AppDbContext.cs
│  ├─ DesignTimeDbContextFactory.cs
│  ├─ Configurations/       Fluent API configuration, one class per entity
│  ├─ Migrations/
│  └─ DbSeeder.cs           Sample data (idempotent)
├─ Services/                Business logic and validation
└─ UI/                      All Console.WriteLine/ReadLine calls live here only
```

Layering rules: `UI` only calls `Services`; `Services` never call `Console`; `Models` does not depend on EF Core.

## Migrations

```bash
dotnet ef migrations add <Name> --output-dir Data/Migrations
dotnet ef database update
```

The application also applies migrations automatically at startup, so running `dotnet ef database update` before `dotnet run` is not required.
