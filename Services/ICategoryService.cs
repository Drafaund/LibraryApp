using LibraryApp.Common;
using LibraryApp.Domain.Entities;

namespace LibraryApp.Services;

public interface ICategoryService
{
    Task<Result<List<Category>>> ListAsync();
    Task<Result<Category>> GetByIdAsync(int id);
    Task<Result<Category>> CreateAsync(string? name);
    Task<Result> UpdateAsync(int id, string? name);
    Task<Result> DeleteAsync(int id);
}
