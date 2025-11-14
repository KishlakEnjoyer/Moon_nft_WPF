using Microsoft.AspNetCore.Mvc;
using Moon_NFT_WPFAPI.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Moon_NFT_WPFAPI.Services;
using Moon_NFT_WPFAPI.DTOs;
using Moon_NFT_WPFAPI.EmailModels;

namespace Moon_NFT_WPFAPI.Controllers
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
            var user = await MoonNftDbContext.GetContext.Users
                .AsNoTracking()
                .Include(u => u.TransactionIdBuyerNavigations)
                .Include(u => u.TransactionIdSalerNavigations)
                .Include(u => u.PresentOwneridPresentNavigations)
                    .ThenInclude(p => p.IdPresentCollectionNavigation)
                .Include(u => u.PresentOwneridPresentNavigations)
                    .ThenInclude(p => p.IdModelNavigation)
                .Include(u => u.PresentOwneridPresentNavigations)
                    .ThenInclude(p => p.IdBackgroundNavigation)
                .Include(u => u.PresentOwneridPresentNavigations)
                    .ThenInclude(p => p.IdSymbolNavigation)
                .Include(u => u.IdLots)
                    .ThenInclude(l => l.IdPresentNavigation)
                        .ThenInclude(p => p.IdPresentCollectionNavigation)
                .Include(u => u.IdLots)
                    .ThenInclude(l => l.IdSalerNavigation)
                .Include(u => u.PresentAuthoridPresentNavigations)
                    .ThenInclude(p => p.IdPresentCollectionNavigation)
                .FirstOrDefaultAsync(u => u.IdUser == userId);

            if (user is null)
            {
                return BadRequest("Такой пользователь не найден!");
            }

            var activeLots = await MoonNftDbContext.GetContext.Lots
                .Where(l => user.PresentOwneridPresentNavigations.Select(p => p.IdPresent).Contains(l.IdPresent))
                .ToListAsync();

            var userLots = await MoonNftDbContext.GetContext.Lots.Where(l => user.IdUser == l.IdSaler).ToListAsync();

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
                PresentsUser = user.PresentOwneridPresentNavigations.Select(p => new presentDTO
                {
                    IdPresent = p.IdPresent,
                    OwneridPresent = p.OwneridPresent,
                    OwnernamePresent = p.OwneridPresentNavigation?.NicknameUser ?? string.Empty,
                    IdPresentCollection = p.IdPresentCollection,
                    CollectionName = p.IdPresentCollectionNavigation?.NamePresentCollection ?? string.Empty,
                    IdModel = p.IdModel,
                    ModelName = p.IdModelNavigation?.NameModel ?? string.Empty,
                    IdBackground = p.IdBackground,
                    BackgroundName = p.IdBackgroundNavigation?.NameBackground ?? string.Empty,
                    IdSymbol = p.IdSymbol,
                    SymbolName = p.IdSymbolNavigation?.NameSymbol ?? string.Empty,
                    displayNum = p.displayNum,
                    ImagePresent = p.ImagePresent ?? Array.Empty<byte>(),
                    UpgradePresent = p.UpgradePresent,
                    DateUpgradePresent = p.DateUpgradePresent,
                    priceLotPresent = activeLots.FirstOrDefault(l => l.IdPresent == p.IdPresent)?.PriceLot
                }).ToList(),
                CartUser = user.IdLots.Select(l => new LotDTO
                {
                    IdLot = l.IdLot,
                    IdPresent = l.IdPresent,
                    DateUpgradePresent = l.IdPresentNavigation?.DateUpgradePresent,
                    _collectionId = l.IdPresentNavigation?.IdPresentCollection ?? 0,
                    _collectionName = l.IdPresentNavigation?.IdPresentCollectionNavigation?.NamePresentCollection ?? string.Empty,
                    _modelId = l.IdPresentNavigation?.IdModel ?? 0,
                    _modelName = l.IdPresentNavigation?.IdModelNavigation?.NameModel ?? string.Empty,
                    _bgId = l.IdPresentNavigation?.IdBackground ?? 0,
                    _bgName = l.IdPresentNavigation?.IdBackgroundNavigation?.NameBackground ?? string.Empty,
                    _symbolId = l.IdPresentNavigation?.IdSymbol ?? 0,
                    _symbolName = l.IdPresentNavigation?.IdSymbolNavigation?.NameSymbol ?? string.Empty,
                    IdSaler = l.IdSaler,
                    SalerNickname = l.IdSalerNavigation?.NicknameUser ?? string.Empty,
                    PriceLot = l.PriceLot,
                    statusLot = l.StatusLot
                }).ToList(),
                LotsUser = userLots.Select(l => new LotDTO
                {
                    IdLot = l.IdLot,
                    IdPresent = l.IdPresent,
                    DateUpgradePresent = l.IdPresentNavigation?.DateUpgradePresent,
                    _collectionId = l.IdPresentNavigation?.IdPresentCollection ?? 0,
                    _collectionName = l.IdPresentNavigation?.IdPresentCollectionNavigation?.NamePresentCollection ?? string.Empty,
                    _modelId = l.IdPresentNavigation?.IdModel ?? 0,
                    _modelName = l.IdPresentNavigation?.IdModelNavigation?.NameModel ?? string.Empty,
                    _bgId = l.IdPresentNavigation?.IdBackground ?? 0,
                    _bgName = l.IdPresentNavigation?.IdBackgroundNavigation?.NameBackground ?? string.Empty,
                    _symbolId = l.IdPresentNavigation?.IdSymbol ?? 0,
                    _symbolName = l.IdPresentNavigation?.IdSymbolNavigation?.NameSymbol ?? string.Empty,
                    IdSaler = l.IdSaler,
                    SalerNickname = l.IdSalerNavigation?.NicknameUser ?? string.Empty,
                    PriceLot = l.PriceLot,
                    statusLot = l.StatusLot
                }).ToList(),
                TransactionUser = user.TransactionIdBuyerNavigations.Select(t => new transactionDTO
                {
                    IdTransaction = t.IdTransaction,
                    IdSaler = t.IdSaler,
                    IdBuyer = t.IdBuyer,
                    IdPresent = t.IdPresent,
                    DateTransaction = t.DateTransaction,
                    SumTransaction = t.SumTransaction
                })
                .Concat(user.TransactionIdSalerNavigations.Select(t => new transactionDTO
                {
                    IdTransaction = t.IdTransaction,
                    IdSaler = t.IdSaler,
                    IdBuyer = t.IdBuyer,
                    IdPresent = t.IdPresent,
                    DateTransaction = t.DateTransaction,
                    SumTransaction = t.SumTransaction
                }))
                .ToList()
            };

            return Ok(userDto);
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
