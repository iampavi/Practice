using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Practice.DTO;
using Practice.Models;
using Practice.Repositories;
using Practice.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Practice.Services.Background;
using Practice.Mappings;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly Mock<IEmailQueue> _emailQueueMock;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _loggerMock = new Mock<ILogger<UserService>>();
        _emailQueueMock = new Mock<IEmailQueue>();

        _cache = new MemoryCache(new MemoryCacheOptions());

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        _mapper = config.CreateMapper();

        _service = new UserService(
            _repoMock.Object,
            _loggerMock.Object,
            _emailQueueMock.Object,
            _cache,
            _mapper
        );
    }

    [Fact]
    public async Task GetUsersAsync_ReturnsUsers()
    {
        // Arrange
        var query = new UserQuery { PageNumber = 1, PageSize = 5 };

        var users = new List<User>
        {
            new User { Id = 1, Name = "Test", Email = "test@test.com" }
        };

        _repoMock
    .Setup(r => r.GetUsersAsync(It.IsAny<UserQuery>()))
            .ReturnsAsync((users, 1));

        // Act
        var result = await _service.GetUsersAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Item1);
        Assert.Equal(1, result.Item2);
    }
    [Fact]
    public async Task GetUserByIdAsync_Should_Throw_When_NotFound()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetUserByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.GetUserByIdAsync(1));
    }

    [Fact]
    public async Task UpdateUserAsync_Should_Update_User()
    {
        // Arrange
        var dto = new UpdateUserDto
        {
            Id = 1,
            Name = "Updated",
            Email = "updated@test.com",
            Password = "123"
        };

        var existingUser = new User
        {
            Id = 1,
            Name = "Old",
            Email = "old@test.com"
        };

        _repoMock.Setup(r => r.GetUserByIdAsync(1))
            .ReturnsAsync(existingUser);

        _repoMock.Setup(r => r.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.UpdateUserAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated", result.Name);
    }

}