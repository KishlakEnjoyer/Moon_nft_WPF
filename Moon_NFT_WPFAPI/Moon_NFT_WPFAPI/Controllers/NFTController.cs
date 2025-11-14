using Microsoft.AspNetCore.Mvc;
using Moon_NFT_WPFAPI.Models;
using Moon_NFT_WPFAPI.DTOs;
using Moon_NFT_WPFAPI.Services;
using System.Numerics;
using Microsoft.EntityFrameworkCore; 
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
namespace Moon_NFT_WPFAPI.Controllers
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
        public IActionResult GetPresentById(int _presentId)
        {
            var present = MoonNftDbContext.GetContext.Presents
                .Include(p => p.AuthoridPresentNavigation)
                .Include(p => p.IdBackgroundNavigation)
                .Include(p => p.IdModelNavigation)
                .Include(p => p.IdPresentCollectionNavigation)
                .Include(p => p.IdSymbolNavigation)
                .Include(p => p.OwneridPresentNavigation)
                .Include(p => p.Transactions)
                .Where(p => p.IdPresent == _presentId)
                .FirstOrDefault();

            if (present is null)
            {
                return NotFound("Такого подарка нет!");
            }

            var activeLot = MoonNftDbContext.GetContext.Lots
                .Where(l => l.IdPresent == _presentId)
                .Select(l => l.PriceLot)
                .FirstOrDefault();

            var presentDto = new presentDTO
            {
                IdPresent = present.IdPresent,
                OwneridPresent = present.OwneridPresent,
                OwnernamePresent = present.OwneridPresentNavigation?.NicknameUser ?? string.Empty,
                IdPresentCollection = present.IdPresentCollection,
                CollectionName = present.IdPresentCollectionNavigation?.NamePresentCollection ?? string.Empty,
                displayNum = $"#{present.NumPresent} / {present.IdPresentCollectionNavigation.LimitPresentCollection}",
                IdModel = present.IdModel,
                ModelName = present.IdModelNavigation != null ? present.IdModelNavigation.NameModel : null,
                IdBackground = present.IdBackground,
                BackgroundName = present.IdBackgroundNavigation != null ? present.IdBackgroundNavigation.NameBackground : null,
                IdSymbol = present.IdSymbol,
                SymbolName = present.IdSymbolNavigation != null ? present.IdSymbolNavigation.NameSymbol : null,
                ImagePresent = present.ImagePresent,
                UpgradePresent = present.UpgradePresent,
                DateUpgradePresent = present.DateUpgradePresent,
                priceLotPresent = activeLot
            };

            return Ok(presentDto);
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
                if (currLot != null)
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

                    MoonNftDbContext.GetContext.Lots.Remove(currLot);
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
        public List<collectionDTO> getAllVid()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var collections = context.Presentcollections.Select(c => new collectionDTO
                {
                    IdPresentCollections = c.IdPresentCollections,
                    NamePresentCollection = c.NamePresentCollection,
                    ImagePresentcollections = c.ImagePresentcollections,
                    LimitPresentCollection = c.LimitPresentCollection,
                    AvailableCount = c.AvailableCount,
                    PricePresentCollection = c.PricePresentCollection
                }).ToList();
                return collections;
            }
            catch
            {
                return new List<collectionDTO>();
            }
        }

        [HttpGet("GetAllModelsForCollection")]
        public Presentcollection getAllModels(int idCurrColl)
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var collection = context.Presentcollections
                    .Where(c => c.IdPresentCollections == idCurrColl)
                    .Select(c => new Presentcollection
                    {
                        IdModels = c.IdModels
                    })
                    .FirstOrDefault();
                return collection ?? new Presentcollection();
            }
            catch
            {
                return new Presentcollection();
            }
        }

        [HttpGet("GetAllModels")]
        public List<modelDTO> getAllAllModels()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var models = context.Models.Select(m => new modelDTO
                {
                    IdModel = m.IdModel,
                    NameModel = m.NameModel,
                    ImageModel = m.ImageModel
                }).ToList();
                return models;
            }
            catch
            {
                return new List<modelDTO>();
            }
        }

        [HttpGet("GetAllBG")]
        public List<bgDTO> getAllBg()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var bgs = context.Backgrounds.Select(b => new bgDTO
                {
                    IdBackground = b.IdBackground,
                    NameBackground = b.NameBackground,
                    ColorBackground = b.ColorBackground
                }).ToList();
                return bgs;
            }
            catch
            {
                return new List<bgDTO>();
            }
        }

        [HttpGet("GetAllSym")]
        public List<symbolDTO> getAllSymbols()
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var symbols = context.Symbols.Select(s => new symbolDTO
                {
                    IdSymbol = s.IdSymbol,
                    NameSymbol = s.NameSymbol,
                    ImageSymbol = s.ImageSymbol
                }).ToList();
                return symbols;
            }
            catch
            {
                return new List<symbolDTO>();
            }
        }

        [HttpGet("GetAllActiveLots")]
        public List<LotDTO> getAllActiveLots(string? search, string _collection, string _model, string _background, string _symbol, string _sort)
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
                    .Include(l => l.IdSalerNavigation) 
                    .Where(l => l.StatusLot == "Active")
                    .Select(l => new LotDTO
                    {
                        IdLot = l.IdLot,
                        IdPresent = l.IdPresent,
                        _collectionId = l.IdPresentNavigation.IdPresentCollection,
                        _collectionName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                        _modelId = l.IdPresentNavigation.IdModel ?? 0,
                        _modelName = l.IdPresentNavigation.IdModelNavigation.NameModel,
                        _bgId = l.IdPresentNavigation.IdBackground ?? 0,
                        _bgName = l.IdPresentNavigation.IdBackgroundNavigation.NameBackground,
                        _symbolId = l.IdPresentNavigation.IdSymbol ?? 0,
                        _symbolName = l.IdPresentNavigation.IdSymbolNavigation.NameSymbol,
                        IdSaler = l.IdSaler,
                        SalerNickname = l.IdSalerNavigation.NicknameUser, 
                        PriceLot = l.PriceLot,
                        DateUpgradePresent = l.IdPresentNavigation.DateUpgradePresent
                    })
                    .AsQueryable();

                if (!string.IsNullOrEmpty(_collection) && _collection != "Все коллекции")
                {
                    query = query.Where(l => l._collectionName == _collection);
                }
                if (!string.IsNullOrEmpty(_model) && _model != "Все модели")
                {
                    query = query.Where(l => l._modelName == _model);
                }
                if (!string.IsNullOrEmpty(_background) && _background != "Все фоны")
                {
                    query = query.Where(l => l._bgName == _background);
                }
                if (!string.IsNullOrEmpty(_symbol) && _symbol != "Все узоры")
                {
                    query = query.Where(l => l._symbolName == _symbol);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var lowerSearch = search.ToLower();

                    query = query.Where(l =>
                        l._collectionName.ToLower().Contains(lowerSearch) ||
                        l._modelName.ToLower().Contains(lowerSearch)
                    );
                }

                if (_sort != null && _sort != "Нет (Сортировка)")
                {
                    query = _sort switch
                    {
                        "По цене (По убыванию)" => query.OrderByDescending(l => l.PriceLot),
                        "По цене (По возрастанию)" => query.OrderBy(l => l.PriceLot),
                        "По дате улучшения (По убыванию)" => query.OrderByDescending(l => l.DateUpgradePresent),
                        "По дате улучшения (По возрастанию)" => query.OrderBy(l => l.DateUpgradePresent),
                        _ => query.OrderBy(l => l.IdLot)
                    };
                }

                return query.ToList();
            }
            catch
            {
                return new List<LotDTO>();
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
            System.Drawing.Color sysColor = bgService.HexToColor(bgcolor);
            var backgroundColor = SixLabors.ImageSharp.Color.FromRgb(sysColor.R, sysColor.G, sysColor.B);

            // Размеры результата
            int width = 800;
            int height = 800;

            Image<Rgba32>? imgModel = null;
            Image<Rgba32>? imgSymbol = null;
            Image<Rgba32>? resultImage = null;

            try
            {
                using var modelStream = new MemoryStream(randomModel.ImageModel);
                imgModel = Image.Load<Rgba32>(modelStream);

                using var symbolStream = new MemoryStream(randomSymbol.ImageSymbol);
                imgSymbol = Image.Load<Rgba32>(symbolStream);

                // Масштабируем узор
                int symbolWidth = 150;
                int symbolHeight = 150;
                int stepX = 250;
                int stepY = 250;

                var scaledSymbol = imgSymbol.Clone(ctx => ctx.Resize(symbolWidth, symbolHeight));

                // Прозрачность
                var symbolWithTransparency = scaledSymbol.Clone();
                symbolWithTransparency.Mutate(ctx => ctx.ProcessPixelRowsAsVector4(pixelRow =>
                {
                    for (int i = 0; i < pixelRow.Length; i++)
                    {
                        ref var pixel = ref pixelRow[i];
                        pixel.W *= 0.5f;
                    }
                }));

                // Создаём результатное изображение
                resultImage = new Image<Rgba32>(width, height);
                resultImage.Mutate(ctx => ctx.BackgroundColor(backgroundColor));

                // Рисуем сетку замков
                int offsetX = 50;
                int offsetY = 50;

                for (int x = offsetX; x < width - offsetX; x += stepX)
                {
                    for (int y = offsetY; y < height - offsetY; y += stepY)
                    {
                        var matrix = Matrix3x2.CreateTranslation(-symbolWidth / 2.0f, -symbolHeight / 2.0f)
                                           * Matrix3x2.CreateRotation(MathF.PI * 340.0f / 180.0f)
                                           * Matrix3x2.CreateTranslation(x + symbolWidth / 2.0f, y + symbolHeight / 2.0f);

                        resultImage.Mutate(ctx =>
                        {
                            ctx.Transform(matrix);
                            ctx.DrawImage(symbolWithTransparency, new Point(0, 0), 1.0f);
                        });
                    }
                }

                // Масштабируем модель
                int modelWidth = imgModel.Width / 2;
                int modelHeight = imgModel.Height / 2;

                var scaledModel = imgModel.Clone(ctx => ctx.Resize(modelWidth, modelHeight));

                // Центрируем модель
                int X = (width - modelWidth) / 2;
                int Y = (height - modelHeight) / 2;

                resultImage.Mutate(ctx => ctx.DrawImage(scaledModel, new Point(X, Y), 1.0f));

                // Сохраняем в байты
                byte[] resultBytes;
                using (var ms = new MemoryStream())
                {
                    resultImage.Save(ms, PngFormat.Instance);
                    resultBytes = ms.ToArray();
                }

                // Обновляем сущность
                currentPresent.DateUpgradePresent = DateOnly.FromDateTime(DateTime.Today);
                currentPresent.ImagePresent = resultBytes;
                currentPresent.IdBackground = randomBg.IdBackground;
                currentPresent.IdModel = randomModel.IdModel;
                currentPresent.IdSymbol = randomSymbol.IdSymbol;
                currentPresent.UpgradePresent = 1;

                MoonNftDbContext.GetContext.SaveChanges();

                return Ok("Подарок улучшен!");
            }
            catch (ImageFormatException ex)
            {
                Console.WriteLine($"Ошибка формата изображения: {ex.Message}");
                return BadRequest("Неверный формат изображения символа или модели.");
            }
            catch (NotSupportedException ex)
            {
                Console.WriteLine($"Формат изображения не поддерживается: {ex.Message}");
                return BadRequest("Неподдерживаемый формат изображения символа или модели.");
            }
            finally
            {
                imgModel?.Dispose();
                imgSymbol?.Dispose();
                resultImage?.Dispose();
            }
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
            var _currlot = MoonNftDbContext.GetContext.Lots.FirstOrDefault(l => l.IdPresent == _presentId && l.StatusLot == "Active");

            if (_currlot is null)
            {
                return BadRequest("Такого лота не существует");
            }

            MoonNftDbContext.GetContext.Lots.Remove(_currlot);
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok("Лот удалён!");
        }
    }
}