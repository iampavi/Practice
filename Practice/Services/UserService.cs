using AutoMapper;
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
    private readonly IMapper _mapper;
    public UserService(IUserRepository repo, ILogger<IUserService> logger ,IEmailQueue emailQueue ,IMemoryCache memoryCache,IMapper mapper)
    {
        _repo = repo;
        _logger = logger;
        _emailQueue = emailQueue;
        _cache = memoryCache;
        _mapper = mapper;
    }

    public async Task<(List<UserDto>, int)> GetUsersAsync(UserQuery query)
    {
        var cacheKey = $"users_{query.PageNumber}_{query.PageSize}_{query.Search}";

        if (_cache.TryGetValue(cacheKey, out (List<UserDto>, int) cachedData))
        {
            _logger.LogInformation("Returning data from CACHE");
            return cachedData;
        }

        var result = await _repo.GetUsersAsync(query);

        var usersDto = _mapper.Map<List<UserDto>>(result.Item1);

        var mappedResult = (usersDto, result.Item2);

        _cache.Set(cacheKey, mappedResult, TimeSpan.FromMinutes(5));

        // 🔥 Track key
        _cacheKeys.Add(cacheKey);

        _logger.LogInformation("Data stored in CACHE");

        return mappedResult;
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        _logger.LogInformation($"Fetching user with id: {id}");
        var user = await _repo.GetUserByIdAsync(id);

        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        return _mapper.Map<UserDto>(user);
    }

    public async Task CreateUserAsync(CreateUserDto dto)
    {
        var user = _mapper.Map<User>(dto);

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

    public async Task<UserDto> UpdateUserAsync(UpdateUserDto dto)
    {
        if (dto.Id <= 0)
            throw new Exception("Invalid user id");

       
        var existingUser = await _repo.GetUserByIdAsync(dto.Id);

        if (existingUser == null)
            throw new KeyNotFoundException("User not found");

        var oldEmail = existingUser.Email;
        _mapper.Map(dto , existingUser);

        var updatedUser = await _repo.UpdateUserAsync(existingUser);

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

        return _mapper.Map<UserDto>(updatedUser);
    }
    public async Task DeleteUser(int id)
    {
        if (id <= 0)
            throw new Exception("Invalid user id");

        await _repo.DeleteUserAsync(id);

        ClearUserCache();

        _logger.LogInformation($"User Deleted with Id: {id}");
    }
    public async Task<string> UploadFileAsync(IFormFile file)
    {
        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        var path = Path.Combine(folder, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);

        return fileName;
    }

}
