using Microsoft.AspNetCore.Mvc;
using Moon_nft_api.Models;
using Moon_nft_api.DTOs;
using Moon_nft_api.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp; 
using SixLabors.ImageSharp.PixelFormats; 
using SixLabors.ImageSharp.Processing; 
using SixLabors.ImageSharp.Formats.Png; 
using System.Numerics;


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
                var currLot = MoonNftDbContext.GetContext.Lots
                    .Include(l => l.IdUsers) 
                    .FirstOrDefault(l => l.IdLot == idLot);

                if (currLot == null)
                {
                    return BadRequest("Лот не найден!");
                }

                if (currLot.IdSaler == buyerId)
                {
                    return BadRequest("Покупка не удалась. Это ваш подарок.");
                }

                var buyer = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == buyerId);
                var saler = MoonNftDbContext.GetContext.Users.FirstOrDefault(u => u.IdUser == currLot.IdSaler);
                var present = MoonNftDbContext.GetContext.Presents.FirstOrDefault(p => p.IdPresent == currLot.IdPresent);

                if (buyer == null || saler == null || present == null)
                {
                    return BadRequest("Не найдены связанные данные!");
                }

                if (buyer.BalanceUser < currLot.PriceLot)
                {
                    return BadRequest("У вас не хватает баланса!");
                }

                if (currLot.IdUsers != null)
                {
                    foreach (var user in currLot.IdUsers.ToList())
                    {
                        currLot.IdUsers.Remove(user);
                    }
                }

                buyer.BalanceUser -= currLot.PriceLot;
                present.OwneridPresent = buyerId;
                saler.BalanceUser += (float?)(currLot.PriceLot * 0.94);

                MoonNftDbContext.GetContext.Transactions.Add(new Transaction()
                {
                    IdSaler = saler.IdUser,
                    IdBuyer = buyerId,
                    IdPresent = present.IdPresent,
                    DateTransaction = DateOnly.FromDateTime(DateTime.Today),
                    SumTransaction = (float)currLot.PriceLot
                });

                MoonNftDbContext.GetContext.Lots.Remove(currLot);

                MoonNftDbContext.GetContext.SaveChanges();

                return Ok("Покупка совершена успешно!");
            }
            catch (Exception ex)
            {
                return BadRequest($"Покупка не удалась! {ex.Message}");
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
                    return BadRequest("Это ваш лот, в корзину добавить не получится!");
                }

                if (currUser.IdLots.Contains(currLot))
                {
                    return BadRequest("Лот уже в корзине!");
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

        [HttpPost("AddNewCollection")]
        public async Task<IActionResult> AddNewCollection([FromBody] AddCollectionModel model)
        {
            try
            {
                byte[] imageBytes = null;
                if (!string.IsNullOrEmpty(model.ImageBase64))
                {
                    imageBytes = Convert.FromBase64String(model.ImageBase64);
                }

                Presentcollection _collection = new Presentcollection()
                {
                    NamePresentCollection = model.Name,
                    ImagePresentcollections = imageBytes,
                    LimitPresentCollection = model.Limit,
                    AvailableCount = model.Count,
                    PricePresentCollection = model.Price
                };

                MoonNftDbContext.GetContext.Presentcollections.Add(_collection);
                await MoonNftDbContext.GetContext.SaveChangesAsync();

                return Ok("Коллекция добавлена!");
            }
            catch (Exception ex)
            {
                return BadRequest("Какая то ошибка " + ex.Message);
            }
        }

        [HttpGet("GetAllModelsForCollection")]
        public collectionDTO getAllModels(int idCurrColl)
        {
            try
            {
                var context = MoonNftDbContext.GetContext;
                var collection = context.Presentcollections
                    .Where(c => c.IdPresentCollections == idCurrColl)
                    .Select(c => new collectionDTO
                    {
                        Models = c.IdModels.Select(m => new modelDTO
                        {
                            IdModel = m.IdModel,
                            NameModel = m.NameModel,
                            ImageModel = m.ImageModel
                        }).ToList()
                    }).FirstOrDefault();
                return collection ?? new collectionDTO();
            }
            catch
            {
                return new collectionDTO();
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
                    .Select(l => new LotDTO
                    {
                        IdLot = l.IdLot,
                        IdPresent = l.IdPresent,
                        ImagePresent = l.IdPresentNavigation.ImagePresent,
                        _collectionId = l.IdPresentNavigation.IdPresentCollection,
                        _collectionName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                        displayNum = l.IdPresentNavigation.displayNum,
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

                if (_sort != null && _sort != "По умолчанию")
                {
                    query = _sort switch
                    {
                        "По цене ↑" => query.OrderByDescending(l => l.PriceLot),
                        "По цене ↓" => query.OrderBy(l => l.PriceLot),
                        "По дате улучшения ↑" => query.OrderByDescending(l => l.DateUpgradePresent),
                        "По дате улучшения ↓" => query.OrderBy(l => l.DateUpgradePresent),
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

        [HttpGet("GetAllLotsAdmin")]
        public List<LotDTO> getAllLotsAdmin()
        {
            try
            {
                return MoonNftDbContext.GetContext.Lots
                    .Include(l => l.IdPresentNavigation) 
                    .Include(l => l.IdSalerNavigation)
                    .Select(l => new LotDTO
                    {
                        IdLot = l.IdLot,
                        IdPresent = l.IdPresent,
                        ImagePresent = l.IdPresentNavigation.ImagePresent,
                        IdSaler = l.IdSaler,
                        SalerNickname = l.IdSalerNavigation.NicknameUser,
                        PriceLot = l.PriceLot
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<LotDTO>()
                {
                    new LotDTO
                    {
                        SalerNickname = "Ошибка " + ex.Message
                    }
                };
            }
        }

        [HttpGet("GetAllTransactionsAdmin")]
        public List<transactionDTO> getAllTransactionsAdmin() 
        {
            try
            {
                return MoonNftDbContext.GetContext.Transactions
                    .Include(l => l.IdBuyerNavigation)
                    .Include(l => l.IdSalerNavigation)
                    .Include(l => l.IdPresentNavigation)
                    .Select(l => new transactionDTO
                    {
                        IdTransaction = l.IdTransaction,
                        IdPresent = l.IdPresent,
                        ImagePresent = l.IdPresentNavigation.ImagePresent,
                        IdSaler = l.IdSaler,
                        NameSaler = l.IdSalerNavigation.NicknameUser,
                        IdBuyer = l.IdBuyer,
                        NameBuyer = l.IdBuyerNavigation.NicknameUser,
                        DateTransaction = l.DateTransaction,
                        SumTransaction = l.SumTransaction
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                return new List<transactionDTO>()
                {
                    new transactionDTO
                    {
                        NameBuyer = "Ошибка " + ex.Message
                    }
                };
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

            string bgcolor = randomBg.ColorBackground;
            System.Drawing.Color sysColor = bgService.HexToColor(bgcolor);
            var backgroundColor = SixLabors.ImageSharp.Color.FromRgb(sysColor.R, sysColor.G, sysColor.B);

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

                int symbolWidth = 150;
                int symbolHeight = 150;
                int stepX = 250;
                int stepY = 250;

                var scaledSymbol = imgSymbol.Clone(ctx => ctx.Resize(symbolWidth, symbolHeight));

                var symbolWithTransparency = scaledSymbol.Clone();
                symbolWithTransparency.Mutate(ctx => ctx.ProcessPixelRowsAsVector4(pixelRow =>
                {
                    for (int i = 0; i < pixelRow.Length; i++)
                    {
                        ref var pixel = ref pixelRow[i];
                        pixel.W *= 0.5f;
                    }
                }));

                resultImage = new Image<Rgba32>(width, height);
                resultImage.Mutate(ctx => ctx.BackgroundColor(backgroundColor));

                int offsetX = 50;
                int offsetY = 50;

                // Создаем повернутый символ один раз
                using var rotatedSymbol = new Image<Rgba32>(symbolWidth * 2, symbolHeight * 2); // Увеличиваем canvas для вращения
                rotatedSymbol.Mutate(ctx => ctx.BackgroundColor(SixLabors.ImageSharp.Color.Transparent));

                // Рисуем символ в центре увеличенного canvas и поворачиваем
                rotatedSymbol.Mutate(ctx =>
                {
                    ctx.DrawImage(symbolWithTransparency,
                        new Point((rotatedSymbol.Width - symbolWidth) / 2, (rotatedSymbol.Height - symbolHeight) / 2),
                        1.0f);
                    ctx.Rotate(-35f); // Поворот против часовой стрелки на 35 градусов
                });

                for (int x = offsetX; x < width - offsetX; x += stepX)
                {
                    for (int y = offsetY; y < height - offsetY; y += stepY)
                    {
                        // Вычисляем смещение для центрирования повернутого символа
                        int drawX = x - (rotatedSymbol.Width - symbolWidth) / 2;
                        int drawY = y - (rotatedSymbol.Height - symbolHeight) / 2;

                        resultImage.Mutate(ctx => ctx.DrawImage(rotatedSymbol, new Point(drawX, drawY), 1.0f));
                    }
                }

                int modelWidth = imgModel.Width / 2;
                int modelHeight = imgModel.Height / 2;

                var scaledModel = imgModel.Clone(ctx => ctx.Resize(modelWidth, modelHeight));

                int X = (width - modelWidth) / 2;
                int Y = (height - modelHeight) / 2;

                resultImage.Mutate(ctx => ctx.DrawImage(scaledModel, new Point(X, Y), 1.0f));

                byte[] resultBytes;
                using (var ms = new MemoryStream())
                {
                    resultImage.Save(ms, PngFormat.Instance);
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

            var existingLot = MoonNftDbContext.GetContext.Lots
                .FirstOrDefault(l => l.IdPresent == _presentId && l.StatusLot == "Active");

            if (existingLot != null)
            {
                return BadRequest("Этот подарок уже выставлен на продажу!");
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

            return Ok("Лот успешно создан!");
        }

        [HttpPost("TurnOffLot")]
        public IActionResult TurnOffLot(int _presentId)
        {
            var _currlot = MoonNftDbContext.GetContext.Lots
                .Include(l => l.IdUsers) 
                .FirstOrDefault(l => l.IdPresent == _presentId && l.StatusLot == "Active");

            if (_currlot is null)
            {
                return BadRequest("Такого лота не существует");
            }

            if (_currlot.IdUsers != null)
            {
                foreach (var user in _currlot.IdUsers.ToList())
                {
                    _currlot.IdUsers.Remove(user);
                }
            }

            MoonNftDbContext.GetContext.Lots.Remove(_currlot);
            MoonNftDbContext.GetContext.SaveChanges();

            return Ok("Лот удалён!");
        }
    }
}