using Microsoft.AspNetCore.Mvc;
using StoreApi.Infra.Models;
using StoreApi.Infra.DTOs;
using StoreApi.Infra.Repositories;
using System.Collections.Generic;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
    private readonly IUserRepository _repo;

    public UsersController(IUserRepository repo) => _repo = repo;

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetAll() => Ok(_repo.GetAll());

    [HttpGet("{id}")]
    public ActionResult<User> GetById(int id) {
        var user = _repo.GetById(id);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet("username/{username}")]
    public ActionResult<User> GetByUsername(string username) {
        var user = _repo.GetByUsername(username);
        return user == null ? NotFound() : Ok(user);
    }

    [HttpGet("email/{email}")]
    public ActionResult<User> GetByEmail(string email) {
        var user = _repo.GetByEmail(email);
        return user == null ? NotFound() : Ok(user);
    }

            [HttpPost]
     public ActionResult<User> Create(UserDto dto) {
         // Check if username already exists
         if (_repo.UsernameExists(dto.Username)) {
             return BadRequest("Username already exists");
         }

         // Check if email already exists
         if (_repo.EmailExists(dto.Email)) {
             return BadRequest("Email already exists");
         }

         // Hash the password (in production, use proper hashing like BCrypt)
         var passwordHash = HashPassword(dto.Password);

         var user = new User {
             Username = dto.Username,
             Email = dto.Email,
             PasswordHash = passwordHash,
             FirstName = dto.FirstName,
             LastName = dto.LastName
         };

         var created = _repo.Add(user);
         return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
     }

     [HttpPost("login")]
     public ActionResult<LoginResponseDto> Login(LoginDto dto) {
         var passwordHash = HashPassword(dto.Password);
         var user = _repo.AuthenticateUser(dto.Username, passwordHash);

         if (user == null) {
             return Unauthorized(new LoginResponseDto {
                 Message = "Invalid username or password"
             });
         }

         var response = new LoginResponseDto {
             UserId = user.Id,
             Username = user.Username,
             Email = user.Email,
             FirstName = user.FirstName,
             LastName = user.LastName,
             IsActive = user.IsActive,
             Message = "Login successful"
         };

         return Ok(response);
     }

    [HttpPut("{id}")]
    public IActionResult Update(int id, UserUpdateDto dto) {
        var user = _repo.GetById(id);
        if (user == null) return NotFound();

        // Check if email is being changed and if it already exists
        if (dto.Email != user.Email && _repo.EmailExists(dto.Email)) {
            return BadRequest("Email already exists");
        }

        user.Email = dto.Email;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.IsActive = dto.IsActive;

        _repo.Update(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id) {
        var user = _repo.GetById(id);
        if (user == null) return NotFound();

        _repo.Delete(id);
        return NoContent();
    }

    [HttpPatch("{id}/activate")]
    public IActionResult Activate(int id) {
        var user = _repo.GetById(id);
        if (user == null) return NotFound();

        user.IsActive = true;
        _repo.Update(user);
        return NoContent();
    }

    [HttpPatch("{id}/deactivate")]
    public IActionResult Deactivate(int id) {
        var user = _repo.GetById(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        _repo.Update(user);
        return NoContent();
    }

            [HttpGet("active")]
     public ActionResult<IEnumerable<User>> GetActiveUsers() {
         var activeUsers = _repo.GetAll().Where(u => u.IsActive);
         return Ok(activeUsers);
     }

     // Simple password hashing method (in production, use BCrypt or similar)
     private string HashPassword(string password) {
         // This is a simple hash for demonstration
         // In production, use proper hashing like BCrypt.Net-Next
         using var sha256 = System.Security.Cryptography.SHA256.Create();
         var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
         return Convert.ToBase64String(hashedBytes);
     }
 }