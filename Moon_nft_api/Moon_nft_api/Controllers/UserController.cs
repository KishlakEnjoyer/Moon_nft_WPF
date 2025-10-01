using Microsoft.AspNetCore.Mvc;
using Moon_nft_api.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moon_nft_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IEmailService _emailService; // ← внедряем сервис

        public UserController(IEmailService emailService) // ← конструктор
        {
            _emailService = emailService;
        }

        [HttpGet("profile")]
        public async Task<ActionResult<ProfileResponseDto>> GetProfile([FromQuery] long tgId)
        {
            if (tgId <= 0)
                return BadRequest("Некорректный Telegram ID.");

            var user = await MoonNftDbContext.GetContext.Users
                .Where(u => u.IdTgUser == tgId)
                .Select(u => new ProfileResponseDto
                {
                    Nickname = u.NicknameUser,
                    Email = u.EmailUser,
                    Balance = u.BalanceUser
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound("Пользователь не найден. Пройдите регистрацию.");

            return Ok(user);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            var existingUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.EmailUser == req.Email || u.IdTgUser == req.TgId);

            if (existingUser != null)
                return BadRequest(new { message = "Такой пользователь уже существует." });

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(req.Password);

            var newUser = new User
            {
                IdTgUser = req.TgId,
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
                TgId = newUser.IdTgUser,
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
                TgId = user.IdTgUser,
                Nickname = user.NicknameUser,
                Email = user.EmailUser,
                Message = "Вы вошли!"
            });
        }

        [HttpPost("send-verification")]
        public async Task<IActionResult> SendVerification([FromBody] SendVerificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !IsValidEmail(request.Email))
                return BadRequest("Некорректный email.");

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();

            var tempId = Guid.NewGuid().ToString();
            EmailVerificationService.StoreCode(tempId, code, request.Email, TimeSpan.FromMinutes(3));

            try
            {
                await _emailService.SendEmailAsync(
                    toEmail: request.Email,
                    subject: "Верификация email в Moon NFT",
                    htmlBody: $@"
                        <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                            <h2 style='color: #333;'>Ваш код подтверждения</h2>
                            <p style='font-size: 18px;'>Код: <strong style='font-size: 24px; color: #007bff;'>{code}</strong></p>
                            <p>Код действителен <strong>3 минуты</strong>.</p>
                            <p>Если вы не регистрировались — проигнорируйте это письмо.</p>
                        </div>"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] {ex}");
                return StatusCode(500, "Не удалось отправить email. Попробуйте позже.");
            }

            return Ok(new { TempId = tempId });
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyCodeRequest request)
        {
            if (string.IsNullOrEmpty(request.TempId) || string.IsNullOrEmpty(request.Code))
                return BadRequest();

            if (EmailVerificationService.TryGetEmailByCode(request.TempId, request.Code, out string email))
            {
                return Ok(new { Email = email });
            }

            return BadRequest("Неверный или просроченный код.");
        }

        // Вспомогательные DTO
        public class SendVerificationRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class VerifyCodeRequest
        {
            public string TempId { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
        }

        public static class EmailVerificationService
        {
            private static readonly Dictionary<string, (string Code, string Email, DateTime Expiry)> _codes = new();

            public static void StoreCode(string tempId, string code, string email, TimeSpan ttl)
            {
                _codes[tempId] = (code, email, DateTime.UtcNow + ttl);
            }

            public static bool TryGetEmailByCode(string tempId, string inputCode, out string email)
            {
                email = string.Empty;

                if (!_codes.TryGetValue(tempId, out var data))
                    return false;

                if (DateTime.UtcNow > data.Expiry)
                {
                    _codes.Remove(tempId);
                    return false;
                }

                if (data.Code == inputCode)
                {
                    email = data.Email;
                    _codes.Remove(tempId); // одноразовый
                    return true;
                }

                return false;
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
