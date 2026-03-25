using Practice.DTO;
using Practice.Models;

namespace Practice.Services
{
    public interface IUserService
    {
        Task<(List<User>, int)> GetUsersAsync(UserQuery query);
        Task<User?> GetUserByIdAsync(int id);
        Task CreateUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task DeleteUser(int id);

    }
}
