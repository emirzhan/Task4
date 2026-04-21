using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public interface IUserItemRepository
    {
        Task<IEnumerable<UserItem>> GetUserItemsAsync();
        Task<UserItem?> GetUserItemByIdAsync(long id);
        Task AddUserItemAsync(UserItem userItem);
        Task UpdateUserItemAsync(UserItem userItem);
        Task DeleteUserItemAsync(long id);
        Task<bool> UserItemExistsAsync(long id);
        Task<IEnumerable<UserItem>> GetUsersByAgeAsync(int age);
        Task<IEnumerable<UserItem>> GetUsersByHigherAgeAsync(int age);
        Task<IEnumerable<UserItem>> GetUsersByBelowAgeAsync(int age);
    }
}
