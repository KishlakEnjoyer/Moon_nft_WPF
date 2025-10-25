using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Moon_nft_api.DTOs;
using System;
using Moon_nft_api.Services;

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
                if (currLot.IdSaler == buyerId)
                {
                    return BadRequest("Покупка не удалась. Это ваш подарок.");
                }
                if (currLot != null && currLot.StatusLot == "Active")
                {
                    var buyer = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == buyerId);
                    var saler = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == currLot.IdSaler);
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
                    MoonNftDbContext.GetContext.Transactions.Add(new Transaction()
                    {
                        IdSaler = saler.IdUser,
                        IdBuyer = buyerId,
                        IdPresent = present.IdPresent,
                        DateTransaction = DateOnly.FromDateTime(DateTime.Today),
                        SumTransaction = (float)currLot.PriceLot
                    });
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

        [HttpPut("AddLotToCart")]
        public IActionResult addLotToCart(int idUser, int idLot)
        {
            try
            {
                User currUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == idUser);
                Lot currLot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(u => u.IdLot == idLot);
                if (currUser.IdLots.Contains(currLot))
                {
                    return BadRequest("Лот уже в корзине!");
                }
                currUser.IdLots.Add(currLot);
                MoonNftDbContext.GetContext.SaveChanges();
                return Ok("Лот добавлен в корзину!");
            }
            catch
            {
                return BadRequest("Выберете другой лот");
            }
        }

        [HttpPut("RemoveLotToCart")]
        public IActionResult removeLotToCart(int idUser, int idLot)
        {
            try
            {
                User? currUser = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == idUser);
                Lot? currLot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(u => u.IdLot == idLot);
                currUser.IdLots.Remove(currLot);
                MoonNftDbContext.GetContext.SaveChanges();
                return Ok("Лот удалён из корзину!");
            }
            catch
            {
                return BadRequest("Выберете другой лот");
            }
        }

        [HttpGet("GetAllPresentVid")]
        public List<Presentcollection> getAllVid()
        {
            try
            {
                List<Presentcollection> vids = MoonNftDbContext.GetContext.Presentcollections.ToList();
                return vids;
            }
            catch
            {
                return new List<Presentcollection>();
            }
        }

        [HttpGet("GetAllModelsForCollection")]
        public Presentcollection getAllModels(int idCurrColl)
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

        [HttpGet("GetAllModels")]
        public List<Model> getAllAllModels()
        {
            try
            {
                List<Model> models = MoonNftDbContext.GetContext.Models.ToList();
                return models;
            }
            catch
            {
                return new List<Model>();
            }
        }

        [HttpGet("GetAllBG")]
        public List<Background> getAllBg()
        {
            try
            {
                List<Background> bgs = MoonNftDbContext.GetContext.Backgrounds.ToList();
                return bgs;
            }
            catch
            {
                return new List<Background>();
            }
        }

        [HttpGet("GetAllSym")]
        public List<Symbol> getAllSymbols()
        {
            try
            {
                List<Symbol> symbols = MoonNftDbContext.GetContext.Symbols.ToList();
                return symbols;
            }
            catch
            {
                return new List<Symbol>();
            }
        }

        [HttpGet("GetAllActiveLots")]
        public List<Lot> getAllActiveLots(string? search, string _collection, string _model, string _background, string _symbol, string _sort)
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var query = context.Lots
                    .Include(l => l.IdPresentNavigation)
                        .ThenInclude(p => p.IdPresentCollectionNavigation)
                    .Include(l => l.IdPresentNavigation)
                        .ThenInclude(p => p.IdModelNavigation)
                    .Include(l => l.IdPresentNavigation)
                        .ThenInclude(p => p.IdBackgroundNavigation)
                    .Include(l => l.IdPresentNavigation)
                        .ThenInclude(p => p.IdSymbolNavigation)
                    .Where(l => l.StatusLot == "Active")
                    .AsQueryable();

                if (!string.IsNullOrEmpty(_collection) && _collection != "Все коллекции")
                {
                    query = query.Where(l => l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection == _collection);
                }
                if (!string.IsNullOrEmpty(_model) && _model != "Все модели")
                {
                    query = query.Where(l => l.IdPresentNavigation.IdModelNavigation.NameModel == _model);
                }
                if (!string.IsNullOrEmpty(_background) && _background != "Все фоны")
                {
                    query = query.Where(l => l.IdPresentNavigation.IdBackgroundNavigation.NameBackground == _background);
                }
                if (!string.IsNullOrEmpty(_symbol) && _symbol != "Все узоры")
                {
                    query = query.Where(l => l.IdPresentNavigation.IdSymbolNavigation.NameSymbol == _symbol);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var lowerSearch = search.ToLower();

                    query = query.Where(l =>
                        l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection.ToLower().Contains(lowerSearch) ||
                        l.IdPresentNavigation.IdModelNavigation.NameModel.ToLower().Contains(lowerSearch)
                    );
                }

                if (_sort != null && _sort != "Нет (Сортировка)")
                {
                    query = _sort switch 
                    {
                        "По цене (По убыванию)" => query.OrderByDescending(l => l.PriceLot),
                        "По цене (По возрастанию)" => query.OrderBy(l => l.PriceLot),      
                        "По дате улучшения (По убыванию)" => query.OrderByDescending(l => l.IdPresentNavigation.DateUpgradePresent), 
                        "По дате улучшения (По возрастанию)" => query.OrderBy(l => l.IdPresentNavigation.DateUpgradePresent),    
                        _ => query.OrderBy(l => l.IdLot) 
                    };
                }

                var lots = query.ToList();
                return lots;
            }
            catch
            {
                return new List<Lot>();
            }
        }

        [HttpPost("PurchaseNonUpPresent")]
        public IActionResult PurchaseNonUpPresent(int _collectionId, int _userId, string _description)
        {
            User? buyer = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == _userId);
            if (buyer is null)
            {
                return BadRequest("Пользователь не найден!");
            }

            Presentcollection? currentCollection = MoonNftDbContext.GetContext.Presentcollections.FirstOrDefault(c => c.IdPresentCollections == _collectionId);
            if (currentCollection is not null)
            {
                int? availableCount = currentCollection.AvailableCount;
                if (availableCount > 0)
                {
                    if (buyer.BalanceUser < currentCollection.PricePresentCollection)
                    {
                        return BadRequest("Баланса не хватает!");
                    }

                    List<Present> presents = MoonNftDbContext.GetContext.Presents.Where(p => p.IdPresentCollection == _collectionId).ToList();

                    Present newPresent = new Present()
                    {
                        AuthoridPresent = _userId,
                        OwneridPresent = _userId,
                        IdPresentCollection = _collectionId,
                        IdModel = null,
                        IdBackground = null,
                        IdSymbol = null,
                        NumPresent = presents.Count + 1,
                        ImagePresent = null,
                        DescPresent = _description,
                        UpgradePresent = 0,
                        DateUpgradePresent = null
                    };
                    buyer.BalanceUser -= currentCollection.PricePresentCollection;

                    MoonNftDbContext.GetContext.Presents.Add(newPresent);

                    currentCollection.AvailableCount -= 1;

                    MoonNftDbContext.GetContext.SaveChanges();

                    return Ok("Новый подарок создан и куплен");
                }
                else
                {
                    return BadRequest("Эти подарки уже кончились");
                }
            }
            else
            {
                return BadRequest("Такого подарка не существует!");
            }
        }


        [HttpPut("UpgradePresent")]
        public IActionResult UpgradePresent(int presentId)
        {
            Present? currentPresent = MoonNftDbContext.GetContext.Presents
                .FirstOrDefault(p => p.IdPresent == presentId);
            if (currentPresent is null)
            {
                return BadRequest("Такого подарка не существует!");
            }
            if (currentPresent.UpgradePresent == 1)
            {
                return BadRequest("Этот подарок уже улучшен!");
            }
            Random rnd = new Random();

            List<Background> bgs = MoonNftDbContext.GetContext.Backgrounds.ToList();
            List<Symbol> symbols = MoonNftDbContext.GetContext.Symbols.ToList();
            List<Model> models = new List<Model>();
            Model randomModel = null;
            Background randomBg = null;
            Symbol randomSymbol = null;

            Presentcollection? _collection = MoonNftDbContext.GetContext.Presentcollections
                .Include(c => c.IdModels)
                .FirstOrDefault(c => c.IdPresentCollections == currentPresent.IdPresentCollection);

            if (_collection is not null)
            {
                models = _collection.IdModels.ToList();
                if (models.Count == 0) return BadRequest("Нет моделей в коллекции.");

                randomModel = models[rnd.Next(models.Count)];
                randomBg = bgs[rnd.Next(bgs.Count)];
                randomSymbol = symbols[rnd.Next(symbols.Count)];
            }
            else
            {
                return BadRequest("Коллекция не найдена.");
            }

            if (randomModel.ImageModel == null || randomModel.ImageModel.Length == 0)
                return BadRequest("Изображение модели отсутствует.");

            if (randomSymbol.ImageSymbol == null || randomSymbol.ImageSymbol.Length == 0)
                return BadRequest("Изображение символа отсутствует.");

            // Цвет фона
            string bgcolor = randomBg.ColorBackground;
            System.Drawing.Color backgroundColor = bgService.HexToColor(bgcolor);

            // Размеры результата
            int width = 800;
            int height = 800;

            // Загружаем изображения из байтов
            using var imgSymbol = System.Drawing.Image.FromStream(new MemoryStream(randomSymbol.ImageSymbol));
            using var imgModel = System.Drawing.Image.FromStream(new MemoryStream(randomModel.ImageModel));

            // Масштабируем узор
            int symbolWidth = 150;
            int symbolHeight = 150;
            int stepX = 250;
            int stepY = 250;

            using var scaledSymbol = new Bitmap(imgSymbol, new System.Drawing.Size(symbolWidth, symbolHeight));

            // Создаём результатное изображение
            using var result = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(result);

            // Заливаем фон цветом
            graphics.Clear(backgroundColor);

            // Настройка прозрачности для замка
            using var imageAttributes = new ImageAttributes();
            ColorMatrix colorMatrix = new ColorMatrix(
                new float[][]
                {
                new float[] {1f, 0f, 0f, 0f, 0f},
                new float[] {0f, 1f, 0f, 0f, 0f},
                new float[] {0f, 0f, 1f, 0f, 0f},
                new float[] {0f, 0f, 0f, 0.5f, 0f}, // Alpha = 0.5
                new float[] {0f, 0f, 0f, 0f, 1f}
                });

            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            // Рисуем сетку замков
            int offsetX = 50;
            int offsetY = 50;

            for (int x = offsetX; x < width - offsetX; x += stepX)
            {
                for (int y = offsetY; y < height - offsetY; y += stepY)
                {
                    graphics.TranslateTransform(x + scaledSymbol.Width / 2, y + scaledSymbol.Height / 2);
                    graphics.RotateTransform(340);

                    graphics.DrawImage(
                        scaledSymbol,
                        new System.Drawing.Rectangle(-scaledSymbol.Width / 2, -scaledSymbol.Height / 2, scaledSymbol.Width, scaledSymbol.Height),
                        0, 0, scaledSymbol.Width, scaledSymbol.Height,
                        GraphicsUnit.Pixel,
                        imageAttributes
                    );

                    graphics.RotateTransform(-340);
                    graphics.TranslateTransform(-(x + scaledSymbol.Width / 2), -(y + scaledSymbol.Height / 2));
                }
            }

            int modelWidth = imgModel.Width / 2;
            int modelHeight = imgModel.Height / 2;

            using var scaledModel = new Bitmap(imgModel, new System.Drawing.Size(modelWidth, modelHeight));

            // Центрируем модель
            int X = (width - modelWidth) / 2;
            int Y = (height - modelHeight) / 2;


            graphics.DrawImage(scaledModel, X, Y);

            // === 💾 СОХРАНЕНИЕ В БАЗУ ДАННЫХ ===

            // Конвертируем изображение в byte[]
            byte[] resultBytes;
            using (var ms = new MemoryStream())
            {
                result.Save(ms, ImageFormat.Png);
                resultBytes = ms.ToArray();
            }


            currentPresent.DateUpgradePresent = DateOnly.FromDateTime(DateTime.Today);
            currentPresent.ImagePresent = resultBytes;
            currentPresent.IdBackground = randomBg.IdBackground;
            currentPresent.IdModel = randomModel.IdModel;
            currentPresent.IdSymbol = randomSymbol.IdSymbol;
            currentPresent.UpgradePresent = 1;
            
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok("Подарок улучшен!");
        }

        [HttpPost("PublishLot")]
        public IActionResult PublishLot(int _presentId, float _priceLot)
        {
            Present? currPresent = MoonNftDbContext.GetContext.Presents
                .Include(p => p.OwneridPresentNavigation)
                .FirstOrDefault(p => p.IdPresent == _presentId);
            if (currPresent is null) 
            {
                return BadRequest("Такого подарка не существует!");
            }
            if (currPresent.UpgradePresent == 0)
            {
                return BadRequest("Подарок не является уникальным!");
            }
            Lot newLot = new Lot()
            {
                IdPresent = currPresent.IdPresent,
                IdSaler = currPresent.OwneridPresent,
                PriceLot = _priceLot,
                StatusLot = "Active"
            };
            MoonNftDbContext.GetContext.Lots.Add(newLot);
            MoonNftDbContext.GetContext.SaveChanges();
            return Ok();
        }

        [HttpPut("TurnOffLot")]
        public IActionResult TurnOffLot(int _lotId)
        {
            Lot? _currlot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(l => l.IdLot == _lotId);
            if (_currlot is null)
            {
                return BadRequest("Такого лота не существует");
            }
            _currlot.StatusLot = "Deleted";
            MoonNftDbContext.GetContext.SaveChanges();
            return Ok("Лот удалён!");
        }
    }
}
