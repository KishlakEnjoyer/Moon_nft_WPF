using Microsoft.AspNetCore.Mvc;
using Moon_nft_api.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Services;
using Moon_nft_api.DTOs;
using Moon_nft_api.EmailModels;

namespace Moon_nft_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IEmailService _emailService;
        private readonly MoonNftDbContext _context;

        public UserController(IEmailService emailService, MoonNftDbContext context) 
        {
            _emailService = emailService;
            _context = context;

        }

        [HttpGet("GetPresentsForUser")]
        public async Task<ActionResult<List<Present>>> GetPresentsForUser([FromQuery] long usertgid, int limit = 50)
        {
            User? currUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdTgUser == usertgid);
            var presents = MoonNftDbContext.GetContext.Presents
                                    .Where(p => p.OwneridPresent == currUser.IdUser)
                                    .Include(p => p.IdPresentCollectionNavigation)
                                    .Include(p => p.IdModelNavigation)
                                    .Include(p => p.IdSymbolNavigation)
                                    .Include(p => p.IdBackgroundNavigation)
                                    .Select(p => new Present
                                    {
                                        IdPresent = p.IdPresent
                                    })
                                    .ToList();

            return Ok(presents);
        }

        [HttpGet("GetFullProfileInfo")]
        public async Task<ActionResult<UserDTO>> GetFullProfileInfo(int userId)
        {
            // Базовые данные пользователя
            var user = await MoonNftDbContext.GetContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user is null)
            {
                return BadRequest("Такой пользователь не найден!");
            }

            // Последовательная загрузка (убейте Task.WhenAll)
            var presents = await GetRequests.GetUserPresents(userId);
            var carts = await GetRequests.GetCart(userId);
            var lots = await GetRequests.GetUserLots(userId);
            var transactions = await GetRequests.GetUserTransactions(userId);

            var userDto = new UserDTO
            {
                IdUser = user.IdUser,
                IdTgUser = user.IdTgUser,
                EmailUser = user.EmailUser,
                PasswordUser = user.PasswordUser,
                NicknameUser = user.NicknameUser,
                DateRegUser = user.DateRegUser,
                RoleUser = user.RoleUser,
                RatingUser = user.RatingUser,
                BalanceUser = user.BalanceUser,
                PresentsUser = presents,
                CartUser = carts,
                LotsUser = lots,
                TransactionUser = transactions
            };

            return Ok(userDto);
        }

        [HttpGet("GetAllTransactions")]
        public async Task<List<transactionDTO>> getAllTransactions(int userId)
        {
            return await MoonNftDbContext.GetContext.Transactions.Where(t => t.IdBuyer == userId).Select(t => new transactionDTO
            {
                IdTransaction = t.IdTransaction,
                IdSaler = t.IdSaler,
                NameSaler = t.IdSalerNavigation.NicknameUser,
                IdBuyer = t.IdBuyer,
                NameBuyer = t.IdBuyerNavigation.NicknameUser,
                IdPresent = t.IdPresent,
                ImagePresent = t.IdPresentNavigation.ImagePresent,
                CollectionPresent = t.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                displayNum = $"#{t.IdPresentNavigation.NumPresent} / {t.IdPresentNavigation.IdPresentCollectionNavigation.LimitPresentCollection}",
                DateTransaction = t.DateTransaction,
                SumTransaction = t.SumTransaction
            }).ToListAsync();
        }

        [HttpGet("GetAllSales")]
        public async Task<List<transactionDTO>> getAllSales(int userId)
        {
            return await MoonNftDbContext.GetContext.Transactions.Where(t => t.IdSaler == userId).Select(t => new transactionDTO
            {
                IdTransaction = t.IdTransaction,
                IdSaler = t.IdSaler,
                NameSaler = t.IdSalerNavigation.NicknameUser,
                IdBuyer = t.IdBuyer,
                NameBuyer = t.IdBuyerNavigation.NicknameUser,
                IdPresent = t.IdPresent,
                ImagePresent = t.IdPresentNavigation.ImagePresent,
                CollectionPresent = t.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                displayNum = $"#{t.IdPresentNavigation.NumPresent} / {t.IdPresentNavigation.IdPresentCollectionNavigation.LimitPresentCollection}",
                DateTransaction = t.DateTransaction,
                SumTransaction = t.SumTransaction
            }).ToListAsync();
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            if (request.UserId <= 0)
                return BadRequest(new { message = "Некорректный ID пользователя." });

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
                return BadRequest(new { message = "Введите текущий пароль." });

            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return NotFound(new { message = "Пользователь не найден." });

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordUser))
                return Unauthorized(new { message = "Неверный текущий пароль." });

            if (!string.IsNullOrWhiteSpace(request.NewNickname))
            {
                var existingUserWithNickname = await _context.Users
                    .FirstOrDefaultAsync(u => u.NicknameUser == request.NewNickname && u.IdUser != request.UserId);
                if (existingUserWithNickname != null)
                    return BadRequest(new { message = "Никнейм уже занят." });

                user.NicknameUser = request.NewNickname;
            }

            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                if (request.NewPassword != request.ConfirmNewPassword)
                    return BadRequest(new { message = "Новый пароль и подтверждение не совпадают." });

                user.PasswordUser = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Профиль успешно обновлён." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Ошибка при сохранении изменений." });
            }
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
                    UserId = u.IdUser,
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
                RatingUser = 0.0f,
                BalanceUser = 0.0f
            };

            MoonNftDbContext.GetContext.Users.Add(newUser);
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok(new AuthResponse
            {
                TgId = (long)newUser.IdTgUser,
                Nickname = newUser.NicknameUser,
                Email = newUser.EmailUser,
                Message = "Регистрация прошла успешно!"
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            var user = _context.Users.FirstOrDefault(u => u.EmailUser == req.Email);
            if (user == null)
                return Unauthorized(new { message = "Неверная почта или пароль." });

            bool isValid = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordUser);
            if (!isValid)
                return Unauthorized(new { message = "Неверная почта или пароль." });

            return Ok(new AuthResponse
            {
                TgId = (long)user.IdTgUser,
                UserId = user.IdUser,
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
