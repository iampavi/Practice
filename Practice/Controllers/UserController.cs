using Microsoft.AspNetCore.Mvc;
using Practice.DTO;
using Practice.Models;
using Practice.Services;

namespace Practice.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserQuery query)
        {
            var (users, totalCount) = await _userService.GetUsersAsync(query);

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                Data = users
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _userService.CreateUserAsync(dto);

            return StatusCode(201);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody]UpdateUserDto dto)
        {
            if (id != dto.Id)
                return BadRequest("Id mismatch");

            var updatedUser = await _userService.UpdateUserAsync(dto);
            return Ok(updatedUser);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            await _userService.DeleteUser(id);
            return Ok("User deleted successfully");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            var fileName = await _userService.UploadFileAsync(file);

            var url = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new
            {
                FileName = fileName,
                Url = url,
                Size = file.Length
            });
        }

    }
}   
