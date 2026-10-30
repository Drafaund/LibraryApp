using LibraryApp.Common;
using LibraryApp.Models;

namespace LibraryApp.Services;

public interface IBookService
{
    Task<Result<List<Book>>> ListAsync();
    Task<Result<List<Book>>> SearchByTitleAsync(string? keyword);
    Task<Result<Book>> GetByIdAsync(int id);
    Task<Result<Book>> CreateAsync(BookInput input);
    Task<Result> UpdateAsync(int id, BookInput input);
    Task<Result> DeleteAsync(int id);
}
