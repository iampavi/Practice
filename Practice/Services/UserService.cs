using Practice.DTO;
using Practice.Models;
using Practice.Repositories;
using Practice.Services;
using Practice.Services.Background;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly ILogger<IUserService> _logger;
    private readonly IEmailQueue _emailQueue;

    public UserService(IUserRepository repo, ILogger<IUserService> logger ,IEmailQueue emailQueue)
    {
        _repo = repo;
        _logger = logger;
        _emailQueue = emailQueue;
    }

    public async Task<(List<User>, int)> GetUsersAsync(UserQuery query)
    {
        _logger.LogInformation("Fetching users with pagination");
        return await _repo.GetUsersAsync(query);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation($"Fetching user with id: {id}");
        var user = await _repo.GetUserByIdAsync(id);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
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

        
        _logger.LogInformation($"User created with email: {user.Email}");

        
        _emailQueue.Enqueue(user.Email);

        _logger.LogInformation($"Email queued for user: {user.Email}");
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