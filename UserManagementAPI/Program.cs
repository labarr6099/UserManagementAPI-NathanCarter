using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;

namespace UserManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<User> Users =
    [
        new() { Id = 1, Name = "Alice Johnson", Email = "alice@example.com", Department = "Engineering" },
        new() { Id = 2, Name = "Brian Smith", Email = "brian@example.com", Department = "Marketing" },
        new() { Id = 3, Name = "Carol Davis", Email = "carol@example.com", Department = "Finance" }
    ];

    [HttpPost]
    public ActionResult<User> CreateUser(User user)
    {
        if (string.IsNullOrWhiteSpace(user.Name) ||
            string.IsNullOrWhiteSpace(user.Email) ||
            string.IsNullOrWhiteSpace(user.Department))
        {
            return BadRequest("Name, email, and department are required.");
        }

        user.Name = user.Name.Trim();
        user.Email = user.Email.Trim();
        user.Department = user.Department.Trim();

        if (Users.Any(existing =>
            existing.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("Email already exists.");
        }

        user.Id = Users.Count == 0 ? 1 : Users.Max(existing => existing.Id) + 1;
        Users.Add(user);

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id:int}")]
    public ActionResult<User> GetUser(int id)
    {
        var user = Users.FirstOrDefault(existing => existing.Id == id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id:int}")]
    public ActionResult<User> UpdateUser(int id, User updatedUser)
    {
        var user = Users.FirstOrDefault(existing => existing.Id == id);

        if (user is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(updatedUser.Name) ||
            string.IsNullOrWhiteSpace(updatedUser.Email) ||
            string.IsNullOrWhiteSpace(updatedUser.Department))
        {
            return BadRequest("Name, email, and department are required.");
        }

        updatedUser.Email = updatedUser.Email.Trim();

        if (Users.Any(existing =>
            existing.Id != id &&
            existing.Email.Equals(updatedUser.Email, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("Email already exists.");
        }

        user.Name = updatedUser.Name.Trim();
        user.Email = updatedUser.Email;
        user.Department = updatedUser.Department.Trim();

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var user = Users.FirstOrDefault(existing => existing.Id == id);

        if (user is null)
            return NotFound();

        Users.Remove(user);
        return NoContent();
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var users = Users
            .OrderBy(user => user.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(users);
    }
}
public class User
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Department { get; set; } = string.Empty;
}