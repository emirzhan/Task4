using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Repositories
{
    public class Repository : IUserItemRepository
    {
        private readonly UserContext _context;

        public Repository(UserContext context) => _context = context;

        public async Task<IEnumerable<UserItem>> GetUserItemsAsync()
        {
            return await _context.UserItems.ToListAsync();
        }

        public async Task<UserItem?> GetUserItemByIdAsync(long id)
        {
            return await _context.UserItems.FindAsync(id);
        }

        public async Task AddUserItemAsync(UserItem userItem)
        {
            _context.UserItems.Add(userItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserItemAsync(UserItem userItem)
        {
            _context.Entry(userItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserItemAsync(long id)
        {
            var userItem = await _context.UserItems.FindAsync(id);
            if (userItem != null)
            {
                _context.UserItems.Remove(userItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserItemExistsAsync(long id)
        {
            return await _context.UserItems.AnyAsync(e => e.Id == id);
        }

        public async Task<IEnumerable<UserItem>> GetUsersByAgeAsync(int age)
        {
            return await _context.UserItems.Where(user => user.Age == age).ToListAsync();
        }

        public async Task<IEnumerable<UserItem>> GetUsersByHigherAgeAsync(int age)
        {
            return await _context.UserItems.Where(user => user.Age >= age).ToListAsync();
        }

        public async Task<IEnumerable<UserItem>> GetUsersByBelowAgeAsync(int age)
        {
            return await _context.UserItems.Where(user => user.Age <= age).ToListAsync();
        }
    }
}
