using Microsoft.EntityFrameworkCore;
using Moon_nft_api.DTOs;
using Moon_nft_api.Models;

namespace Moon_nft_api.Services
{
    public static class GetRequests
    {
        public static async Task<List<presentDTO>> GetUserPresents(int userId)
        {
            return await MoonNftDbContext.GetContext.Presents
                .AsNoTracking()
                .Where(p => p.OwneridPresent == userId)
                .Include(p => p.IdPresentCollectionNavigation)
                .Include(p => p.IdModelNavigation)
                .Include(p => p.IdBackgroundNavigation)
                .Include(p => p.IdSymbolNavigation)
                .Include(p => p.OwneridPresentNavigation)
                .Select(p => new presentDTO
                {
                    IdPresent = p.IdPresent,
                    OwneridPresent = p.OwneridPresent,
                    OwnernamePresent = p.OwneridPresentNavigation.NicknameUser,
                    IdPresentCollection = p.IdPresentCollection,
                    CollectionName = p.IdPresentCollectionNavigation.NamePresentCollection,
                    IdModel = p.IdModel,
                    ModelName = p.IdModelNavigation.NameModel,
                    IdBackground = p.IdBackground,
                    BackgroundName = p.IdBackgroundNavigation.NameBackground,
                    IdSymbol = p.IdSymbol,
                    SymbolName = p.IdSymbolNavigation.NameSymbol,
                    displayNum = p.displayNum,
                    ImagePresent = p.ImagePresent ?? Array.Empty<byte>(),
                    UpgradePresent = p.UpgradePresent,
                    DateUpgradePresent = p.DateUpgradePresent
                })
                .ToListAsync();
        }

        public static async Task<List<LotDTO>> GetUserLots(int userId)
        {
            return await MoonNftDbContext.GetContext.Lots
                .AsNoTracking()
                .Where(l => l.IdSaler == userId)
                .Include(l => l.IdPresentNavigation)
                    .ThenInclude(p => p.IdPresentCollectionNavigation)
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
                    DateUpgradePresent = l.IdPresentNavigation.DateUpgradePresent,
                    ImagePresent = l.IdPresentNavigation.ImagePresent,
                    _collectionId = l.IdPresentNavigation.IdPresentCollection,
                    _collectionName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                    _modelId = (int)l.IdPresentNavigation.IdModel,
                    _modelName = l.IdPresentNavigation.IdModelNavigation.NameModel,
                    _bgId = (int)l.IdPresentNavigation.IdBackground,
                    _bgName = l.IdPresentNavigation.IdBackgroundNavigation.NameBackground,
                    _symbolId = (int)l.IdPresentNavigation.IdSymbol,
                    _symbolName = l.IdPresentNavigation.IdSymbolNavigation.NameSymbol,
                    IdSaler = l.IdSaler,
                    SalerNickname = l.IdSalerNavigation.NicknameUser,
                    PriceLot = l.PriceLot,
                    displayNum = l.IdPresentNavigation.displayNum
                })
                .ToListAsync();
        }

        public static async Task<List<presentDTO>> GetCart(int userId)
        {
            return await MoonNftDbContext.GetContext.Lots
                .AsNoTracking()
                .Where(l => l.IdUsers.Any(u => u.IdUser == userId)) 
                .Include(l => l.IdSalerNavigation) 
                .Include(l => l.IdPresentNavigation) 
                    .ThenInclude(p => p.IdPresentCollectionNavigation) // Коллекция подарка
                .Include(l => l.IdPresentNavigation)
                    .ThenInclude(p => p.IdModelNavigation) // Модель подарка
                .Include(l => l.IdPresentNavigation)
                    .ThenInclude(p => p.IdBackgroundNavigation) // Фон подарка
                .Include(l => l.IdPresentNavigation)
                    .ThenInclude(p => p.IdSymbolNavigation) // Символ подарка
                .Include(l => l.IdUsers) // Пользователи в корзине (опционально)
                .Select(l => new presentDTO
                {
                    IdPresent = l.IdPresentNavigation.IdPresent, // Берем Id из подарка
                    OwneridPresent = l.IdSaler,
                    OwnernamePresent = l.IdSalerNavigation.NicknameUser,
                    IdPresentCollection = l.IdPresentNavigation.IdPresentCollection,
                    CollectionName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                    IdModel = l.IdPresentNavigation.IdModel,
                    ModelName = l.IdPresentNavigation.IdModelNavigation.NameModel,
                    IdBackground = l.IdPresentNavigation.IdBackground,
                    BackgroundName = l.IdPresentNavigation.IdBackgroundNavigation.NameBackground,
                    IdSymbol = l.IdPresentNavigation.IdSymbol,
                    SymbolName = l.IdPresentNavigation.IdSymbolNavigation.NameSymbol,
                    displayNum = l.IdPresentNavigation.displayNum,
                    ImagePresent = l.IdPresentNavigation.ImagePresent ?? Array.Empty<byte>(),
                    UpgradePresent = l.IdPresentNavigation.UpgradePresent,
                    DateUpgradePresent = l.IdPresentNavigation.DateUpgradePresent,
                    priceLotPresent = l.PriceLot,
                    currLot = new LotDTO
                    {
                        IdLot = l.IdLot,
                        IdPresent = l.IdPresent,
                        DateUpgradePresent = l.IdPresentNavigation.DateUpgradePresent,
                        ImagePresent = l.IdPresentNavigation.ImagePresent,
                        _collectionId = l.IdPresentNavigation.IdPresentCollection,
                        _collectionName = l.IdPresentNavigation.IdPresentCollectionNavigation.NamePresentCollection,
                        _modelId = (int)l.IdPresentNavigation.IdModel,
                        _modelName = l.IdPresentNavigation.IdModelNavigation.NameModel,
                        _bgId = (int)l.IdPresentNavigation.IdBackground,
                        _bgName = l.IdPresentNavigation.IdBackgroundNavigation.NameBackground,
                        _symbolId = (int)l.IdPresentNavigation.IdSymbol,
                        _symbolName = l.IdPresentNavigation.IdSymbolNavigation.NameSymbol,
                        IdSaler = l.IdSaler,
                        SalerNickname = l.IdSalerNavigation.NicknameUser,
                        PriceLot = l.PriceLot,
                        displayNum = l.IdPresentNavigation.displayNum
                    }
                })
                .ToListAsync();
        }

        public static async Task<List<transactionDTO>> GetUserTransactions(int userId)
        {
            var buyerTransactions = await MoonNftDbContext.GetContext.Transactions
                .AsNoTracking()
                .Where(t => t.IdBuyer == userId)
                .Select(t => new transactionDTO
                {
                    IdTransaction = t.IdTransaction,
                    IdSaler = t.IdSaler,
                    IdBuyer = t.IdBuyer,
                    IdPresent = t.IdPresent,
                    DateTransaction = t.DateTransaction,
                    SumTransaction = t.SumTransaction
                })
                .ToListAsync();

            var salerTransactions = await MoonNftDbContext.GetContext.Transactions
                .AsNoTracking()
                .Where(t => t.IdSaler == userId)
                .Select(t => new transactionDTO
                {
                    IdTransaction = t.IdTransaction,
                    IdSaler = t.IdSaler,
                    IdBuyer = t.IdBuyer,
                    IdPresent = t.IdPresent,
                    DateTransaction = t.DateTransaction,
                    SumTransaction = t.SumTransaction
                })
                .ToListAsync();

            return buyerTransactions.Concat(salerTransactions).ToList();
        }
    }
}
