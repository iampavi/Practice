using Microsoft.Extensions.Caching.Memory;
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
    private readonly IMemoryCache _cache;
    private static readonly List<string> _cacheKeys = new();
    public UserService(IUserRepository repo, ILogger<IUserService> logger ,IEmailQueue emailQueue ,IMemoryCache memoryCache)
    {
        _repo = repo;
        _logger = logger;
        _emailQueue = emailQueue;
        _cache = memoryCache;
    }

    public async Task<(List<User>, int)> GetUsersAsync(UserQuery query)
    {
        var cacheKey = $"users_{query.PageNumber}_{query.PageSize}_{query.Search}";

        if (_cache.TryGetValue(cacheKey, out (List<User>, int) cachedData))
        {
            _logger.LogInformation("Returning data from CACHE");
            return cachedData;
        }

        var result = await _repo.GetUsersAsync(query);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        // 🔥 Track key
        _cacheKeys.Add(cacheKey);

        _logger.LogInformation("Data stored in CACHE");

        return result;
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

        await _repo.CreateUserAsync(user);

        ClearUserCache();

        _logger.LogInformation($"User created with email: {user.Email}");

        
        _emailQueue.Enqueue(user.Email);

        _logger.LogInformation($"Email queued for user: {user.Email}");
    }

    private void ClearUserCache()
    {
        foreach (var key in _cacheKeys)
        {
            _cache.Remove(key);
        }

        _cacheKeys.Clear();

        _logger.LogInformation("All user cache cleared");
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        if (user.Id <= 0)
            throw new Exception("Invalid user id");

       
        var existingUser = await _repo.GetUserByIdAsync(user.Id);

        if (existingUser == null)
            throw new Exception("User not found");

        var oldEmail = existingUser.Email;

        var updatedUser = await _repo.UpdateUserAsync(user);

        ClearUserCache();

        _logger.LogInformation(
            "User updated. Id: {Id}, Email: {Email}",
            updatedUser.Id,
            updatedUser.Email
        );

        // CHECK EMAIL CHANGE
        if (oldEmail != updatedUser.Email)
        {
            _logger.LogInformation(
                "Email changed from {OldEmail} to {NewEmail}",
                oldEmail,
                updatedUser.Email
            );

            //  Trigger background job
            _emailQueue.Enqueue(updatedUser.Email);
        }

        return updatedUser;
    }
    public async Task DeleteUser(int id)
    {
        if (id <= 0)
            throw new Exception("Invalid user id");

        await _repo.DeleteUserAsync(id);

        ClearUserCache();

        _logger.LogInformation($"User Deleted with Id: {id}");
    }

}
