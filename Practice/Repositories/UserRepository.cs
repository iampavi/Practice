using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.DTO;
using Practice.Models;

namespace Practice.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<(List<User>, int)> GetUsersAsync(UserQuery query)
        {
            var users = _appDbContext.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(query.Search))
            {
                users = users.Where(x => x.Name.Contains(query.Search));
            }

            // Total count
            var totalCount = await users.CountAsync();

            // Pagination
            var data = await users
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            var user = await _appDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }

        public async Task CreateUserAsync(User user)
        {
            await _appDbContext.Users.AddAsync(user);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == user.Id);

            if (existingUser == null)
            {
                throw new Exception("User not found");
            }

            // Update fields
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Password = user.Password;

            await _appDbContext.SaveChangesAsync();

            return existingUser;
        }
        public async Task DeleteUserAsync(int id)
        {
            var user = await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                throw new Exception("User not found");
            }

            _appDbContext.Users.Remove(user);
            await _appDbContext.SaveChangesAsync();
        }
    } }