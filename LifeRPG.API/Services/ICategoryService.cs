using LifeRPG.Core.Models;

namespace LifeRPG.API.Services
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllAsync();
        Task<Category> CreateAsync(Category category);
    }
}