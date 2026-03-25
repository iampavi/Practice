using Practice.DTO;
using Practice.Models;

namespace Practice.Repositories
{
    public interface IUserRepository
    {
        Task<(List<User>, int)> GetUsersAsync(UserQuery query);
        Task<User?> GetUserByIdAsync(int id);
        Task CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
    }
}
