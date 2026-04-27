using Microsoft.EntityFrameworkCore;
using Practice.Data;
using Practice.DTO;
using Practice.Models;

namespace Practice.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(AppDbContext appDbContext, ILogger<UserRepository> logger)
        {
            _appDbContext = appDbContext;
            _logger = logger;
        }

        public async Task<(List<User>, int)> GetUsersAsync(UserQuery query)
        {
            var users = _appDbContext.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(query.Search))
            {
                users = users.Where(x => x.Name !=null && x.Name.Contains(query.Search));
            }

            // Total count
            var totalCount = await users.CountAsync();

            // Sorting
            var sortBy = query.SortBy?.Trim().ToLower();
            var sortOrder = query.SortOrder?.Trim().ToLower();

            users = sortBy switch
            {
                "id" => sortOrder == "desc"
                    ? users.OrderByDescending(x => x.Id)
                    : users.OrderBy(x => x.Id),

                "name" => sortOrder == "desc"
                    ? users.OrderByDescending(x => x.Name)
                    : users.OrderBy(x => x.Name),

                "email" => sortOrder == "desc"
                    ? users.OrderByDescending(x => x.Email)
                    : users.OrderBy(x => x.Email),

                _ => users.OrderBy(x => x.Id)
            };

            // Pagination
            var data = await users
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (data, totalCount);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _appDbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user with Id: {Id}", id);
                throw;
            }

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