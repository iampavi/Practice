using Practice.Models;
using Practice.Repositories;
using Practice.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;

    public UserService(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _repo.GetAllUserAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _repo.GetUserByIdAsync(id);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Name))
        {
            throw new Exception("User name is required");
        }

        await _repo.CreateUserAsync(user);
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        if (user.Id <= 0)
            throw new Exception("Invalid user id");

        return await _repo.UpdateUserAsync(user);
    }
    public async Task DeleteUser(int id)
    {
        if (id <= 0)
            throw new Exception("Invalid user id");

        await _repo.DeleteUserAsync(id);
    }
}