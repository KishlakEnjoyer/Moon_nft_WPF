using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon_nft_api.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using Moon_nft_api.DTOs;
using System;
using Moon_nft_api.Services;
using Castle.Core.Resource;

namespace Moon_nft_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NFTController : ControllerBase
    {
        [HttpGet("checkPresent")]
        public bool checkPresent(int idPresent)
        {
            if (MoonNftDbContext.GetContext.Lots.FirstOrDefault(l => l.IdPresent == idPresent && l.StatusLot == "Active") is not null)
            {
                return false;
            }
            return true;
        }

        [HttpGet("GetPresentById")]
        public IActionResult getPresentById(int _presentId)
        {
            Present? currPres = MoonNftDbContext.GetContext.Presents.Include(p => p.AuthoridPresentNavigation)
                .Include(p => p.IdBackgroundNavigation)
                .Include(p => p.IdModelNavigation)
                .Include(p => p.IdPresentCollectionNavigation)
                .Include(p => p.IdSymbolNavigation)
                .Include(p => p.OwneridPresentNavigation)
                .Include(p => p.Transactions)
                .FirstOrDefault(p => p.IdPresent == _presentId);

            if (currPres is not null)
            {
                return Ok(currPres);
            }
            return NotFound("Такого подарка нет!");
        }

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
                    if (saler is not null)
                    {
                        saler.BalanceUser += (float?)(currLot.PriceLot * 0.94);
                    }

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
        public IActionResult addLotToCart([FromQuery] int idUser, [FromQuery] int idLot)
        {
            try
            {
                var context = MoonNftDbContext.GetContext; 

                var currUser = context.Users
                    .Include(u => u.IdLots) 
                    .FirstOrDefault(u => u.IdUser == idUser);

                var currLot = context.Lots.FirstOrDefault(l => l.IdLot == idLot);

                if (currUser == null || currLot == null)
                {
                    return BadRequest("Пользователь или лот не найдены.");
                }

                if (currUser.IdUser == currLot.IdSaler)
                {
                    return BadRequest(new { message = "Это ваш лот, в корзину добавить не получится!" });
                }

                if (currUser.IdLots.Contains(currLot))
                {
                    return BadRequest(new { message = "Лот уже в корзине!" });
                }

                currUser.IdLots.Add(currLot);
                context.SaveChanges();

                return Ok("Лот добавлен в корзину!");
            }
            catch (Exception ex) 
            {
                return BadRequest("Выберете другой лот");
            }
        }

        [HttpDelete("RemoveLotToCart")]
        public IActionResult removeLotToCart([FromQuery] int idUser, [FromQuery] int idLot)
        {
            try
            {
                var context = MoonNftDbContext.GetContext; 

                
                var user = context.Users
                    .Include(u => u.IdLots) 
                    .FirstOrDefault(u => u.IdUser == idUser);

                var lot = context.Lots.FirstOrDefault(l => l.IdLot == idLot);

                if (user == null || lot == null)
                {
                    return BadRequest("Пользователь или лот не найдены.");
                }

                user.IdLots.Remove(lot);
                context.SaveChanges();

                return Ok("Лот удалён из корзины!");
            }
            catch (Exception ex) 
            {
                return BadRequest("Выберете другой лот");
            }
        }

        [HttpGet("GetAllPresentVid")]
        public List<PresentcollectionDto> getAllVid()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var collections = context.Presentcollections.Select(c => new PresentcollectionDto
                {
                    IdPresentCollections = c.IdPresentCollections,
                    NamePresentCollection = c.NamePresentCollection,
                    PricePresentCollection = (decimal)c.PricePresentCollection,
                    AvailableCount = c.AvailableCount,
                    LimitPresentCollection = c.LimitPresentCollection,
                    ImagePresentcollections = c.ImagePresentcollections
                }).ToList();
                return collections;
            }
            catch
            {
                return new List<PresentcollectionDto>();
            }
        }

        [HttpGet("GetAllModelsForCollection")]
        public PresentcollectionDto getAllModels(int idCurrColl)
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var collection = context.Presentcollections
                    .Where(c => c.IdPresentCollections == idCurrColl)
                    .Select(c => new PresentcollectionDto
                    {
                        IdPresentCollections = c.IdPresentCollections,
                        NamePresentCollection = c.NamePresentCollection,
                        PricePresentCollection = (decimal)c.PricePresentCollection,
                        AvailableCount = c.AvailableCount,
                        LimitPresentCollection = c.LimitPresentCollection,
                        ImagePresentcollections = c.ImagePresentcollections,
                        IdModels = c.IdModels.Select(m => new ModelDto
                        {
                            IdModel = m.IdModel,
                            NameModel = m.NameModel,
                            ImageModel = m.ImageModel
                        }).ToList()
                    })
                    .FirstOrDefault();
                return collection ?? new PresentcollectionDto();
            }
            catch
            {
                return new PresentcollectionDto();
            }
        }

        [HttpGet("GetAllModels")]
        public List<ModelDto> getAllAllModels()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var models = context.Models.Select(m => new ModelDto
                {
                    IdModel = m.IdModel,
                    NameModel = m.NameModel,
                    ImageModel = m.ImageModel
                }).ToList();
                return models;
            }
            catch
            {
                return new List<ModelDto>();
            }
        }

        [HttpGet("GetAllBG")]
        public List<BackgroundDto> getAllBg()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var bgs = context.Backgrounds.Select(b => new BackgroundDto
                {
                    IdBackground = b.IdBackground,
                    NameBackground = b.NameBackground,
                    ColorBackground = b.ColorBackground
                }).ToList();
                return bgs;
            }
            catch
            {
                return new List<BackgroundDto>();
            }
        }

        [HttpGet("GetAllSym")]
        public List<SymbolDto> getAllSymbols()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var symbols = context.Symbols.Select(s => new SymbolDto
                {
                    IdSymbol = s.IdSymbol,
                    NameSymbol = s.NameSymbol,
                    ImageSymbol = s.ImageSymbol
                }).ToList();
                return symbols;
            }
            catch
            {
                return new List<SymbolDto>();
            }
        }

        [HttpGet("GetAllActiveLots")]
        public List<LotDto> getAllActiveLots(string? search, string _collection, string _model, string _background, string _symbol, string _sort)
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var query = context.Lots
            .Include(l => l.IdPresentNavigation)
                .ThenInclude(p => p.IdPresentCollectionNavigation) 
                .ThenInclude(c => c.IdModels)
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

                var lots = query.Select(l => new LotDto
                {
                    IdLot = l.IdLot,
                    IdPresent = l.IdPresent,
                    IdSaler = l.IdSaler,
                    PriceLot = (float)l.PriceLot,
                    StatusLot = l.StatusLot,
                    PresentName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                    ModelName = l.IdPresentNavigation.IdModelNavigation.NameModel,
                    BackgroundName = l.IdPresentNavigation.IdBackgroundNavigation.NameBackground,
                    SymbolName = l.IdPresentNavigation.IdSymbolNavigation.NameSymbol,
                    ImagePresent = l.IdPresentNavigation.ImagePresent,
                    DateUpgradePresent = l.IdPresentNavigation.DateUpgradePresent,
                    NumPresent = l.IdPresentNavigation.NumPresent, 
                    PresentCollectionLimit = l.IdPresentNavigation.IdPresentCollectionNavigation.LimitPresentCollection 
                }).ToList();

                return lots;
            }
            catch
            {
                return new List<LotDto>();
            }
        }

        [HttpPost("PurchaseNonUpPresent")]
        public IActionResult PurchaseNonUpPresent(PurchaseRequest request)
        {
            User? buyer = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == request.UserId);
            if (buyer is null)
            {
                return BadRequest("Пользователь не найден!");
            }

            Presentcollection? currentCollection = MoonNftDbContext.GetContext.Presentcollections.FirstOrDefault(c => c.IdPresentCollections == request.CollectionId);
            if (currentCollection is not null)
            {
                int? availableCount = currentCollection.AvailableCount;
                if (availableCount > 0)
                {
                    if (buyer.BalanceUser < currentCollection.PricePresentCollection)
                    {
                        return BadRequest("Баланса не хватает!");
                    }

                    List<Present> presents = MoonNftDbContext.GetContext.Presents.Where(p => p.IdPresentCollection == request.CollectionId).ToList();

                    Present newPresent = new Present()
                    {
                        AuthoridPresent = request.UserId,
                        OwneridPresent = request.UserId,
                        IdPresentCollection = request.CollectionId,
                        IdModel = null,
                        IdBackground = null,
                        IdSymbol = null,
                        NumPresent = presents.Count + 1,
                        ImagePresent = currentCollection.ImagePresentcollections,
                        DescPresent = null,
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
                // randomBg = bgs[8];
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
            var currPresent = MoonNftDbContext.GetContext.Presents
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

            var newLot = new Lot()
            {
                IdPresent = currPresent.IdPresent,
                IdPresentNavigation = currPresent,
                IdSaler = currPresent.OwneridPresent,
                PriceLot = _priceLot,
                StatusLot = "Active"
            };

            MoonNftDbContext.GetContext.Lots.Add(newLot);
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok();
        }

        [HttpPost("TurnOffLot")]
        public IActionResult TurnOffLot(int _presentId)
        {
            Lot? _currlot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(l => l.IdPresent == _presentId && l.StatusLot == "Active");
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