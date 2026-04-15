using Practice.DTO;
using Practice.Models;

namespace Practice.Services
{
    public interface IUserService
    {
        Task<(List<UserDto>, int)> GetUsersAsync(UserQuery query);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task CreateUserAsync(CreateUserDto dto);
        Task<UserDto> UpdateUserAsync(UpdateUserDto dto);
        Task DeleteUser(int id);
        Task<string> UploadFileAsync(IFormFile file);

    }
}
