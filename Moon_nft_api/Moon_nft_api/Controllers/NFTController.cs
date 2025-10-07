using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Moon_nft_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NFTController : ControllerBase
    {
        [HttpPut("PurchasePresent")]
        public IActionResult buyPresent(int idLot, int buyerId)
        {
            try
            {
                var currLot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(l => l.IdLot == idLot);
                if (currLot != null && currLot.StatusLot == "Active")
                {
                    var buyer = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdTgUser == buyerId);
                    var saler = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdTgUser == currLot.IdSaler);
                    var present = MoonNftDbContext.GetContext.Presents.FirstOrDefault(p => p.IdPresent == currLot.IdPresent);

                    if (buyer.BalanceUser >= currLot.PriceLot)
                    {
                       buyer.BalanceUser -= currLot.PriceLot;
                    }
                    else
                    {
                        return BadRequest("У вас не хватает баланса!");
                    }

                    present.OwneridPresent = buyerId;

                    saler.BalanceUser += (float?)(currLot.PriceLot * 0.94);

                    currLot.StatusLot = "Sold";
                    MoonNftDbContext.GetContext.SaveChanges();

                    return Ok("Покупка совершена успешно!");
                }
                return BadRequest("Покупка не удалась!");
            }
            catch
            {
                return BadRequest("Покупка не удалась!");
            }
        }

        [HttpGet("GetAllPresentVid")]
        public List<Presentcollection> getAllVid()
        {
            try
            {
                var vids = MoonNftDbContext.GetContext.Presentcollections.ToList();
                return vids;
            }
            catch
            {
                return new List<Presentcollection>();
            }
        }

        [HttpGet("GetAllModelsForCollection")]
        public Presentcollection getAllVid(int idCurrColl)
        {
            try
            {
                Presentcollection models = MoonNftDbContext.GetContext.Presentcollections.Include(m => m.IdModels).FirstOrDefault(m => m.IdPresentCollections == idCurrColl);
                return models;
            }
            catch
            {
                return new Presentcollection();
            }
        }


    }
}
