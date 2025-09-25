using Microsoft.AspNetCore.Mvc;
using Moon_nft_api.Models;
using System.Data;
using Dapper;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moon_nft_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BalanceController : ControllerBase
    {
        

        [HttpPost("topup")]
        public async Task<IActionResult> TopUpBalance([FromBody] TopUpRequest request)
        {
            var currUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.EmailUser == request.Email);
            if (currUser is not null)
            {
                currUser.BalanceUser += request.Amount;
                MoonNftDbContext.GetContext.SaveChanges();
                return Ok($"Пополнение прошло успешно на {request.Amount}");
            }
            return BadRequest($"Не удалось пополнить баланс пользователю {request.Email}");
        }
    }

   
}
