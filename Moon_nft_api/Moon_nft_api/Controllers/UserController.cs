using Microsoft.AspNetCore.Mvc;
using Moon_nft_api.Models;
using BCrypt.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moon_nft_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        //private readonly MoonNftDbContext _context;

        //public UserController(MoonNftDbContext context)
        //{
        //    _context = context;
        //}

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            var existingUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.EmailUser == req.Email);

            if (existingUser != null)
                return BadRequest(new { message = "Такой пользователь уже существует." });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var newUser = new User
            {
                EmailUser = req.Email,
                PasswordUser = hashedPassword,
                NicknameUser = req.Nickname,
                DateRegUser = DateOnly.FromDateTime(DateTime.Now),
                RoleUser = "User",
                RatingUser = 0.0f
            };

            MoonNftDbContext.GetContext.Users.Add(newUser);
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok(new AuthResponse
            {
                Id = newUser.IdUser,
                Nickname = newUser.NicknameUser,
                Email = newUser.EmailUser,
                Message = "Регистрация прошла успешно!"
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var user = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.EmailUser == req.Email);
            if (user == null)
                return Unauthorized(new { message = "Неверная почта или пароль." });

            bool isValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordUser);
            if (!isValid)
                return Unauthorized(new { message = "Неверная почта или пароль." });

            return Ok(new AuthResponse
            {
                Id = user.IdUser,
                Nickname = user.NicknameUser,
                Email = user.EmailUser,
                Message = "Вы вошли!"
            });
        }
    }
}
