using LibraryApp.Common;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Services;

public interface IAuthorService
{
    Task<Result<List<Author>>> ListAsync();
    Task<Result<Author>> GetByIdAsync(int id);
    Task<Result<Author>> CreateAsync(string? name);
    Task<Result> UpdateAsync(int id, string? name);
    Task<Result> DeleteAsync(int id);
}
