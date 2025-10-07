using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Moon_nft_application.Models;

public partial class MoonNftDbContext : DbContext
{
    private static MoonNftDbContext _context;

    public static MoonNftDbContext GetContext
    {
        get
        {
            if (_context is null)
            {
                _context = new MoonNftDbContext();
            }
            return _context;
        }
    }
    public MoonNftDbContext()
    {
    }

    public MoonNftDbContext(DbContextOptions<MoonNftDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Background> Backgrounds { get; set; }

    public virtual DbSet<Lot> Lots { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<Present> Presents { get; set; }

    public virtual DbSet<Presentcollection> Presentcollections { get; set; }

    public virtual DbSet<Symbol> Symbols { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;user=root;password=root;database=moon_nft_db", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.1.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Background>(entity =>
        {
            entity.HasKey(e => e.IdBackground).HasName("PRIMARY");

            entity.ToTable("backgrounds");

            entity.Property(e => e.IdBackground).HasColumnName("idBackground");
            entity.Property(e => e.ColorBackground)
                .HasMaxLength(7)
                .HasColumnName("colorBackground");
            entity.Property(e => e.NameBackground)
                .HasMaxLength(45)
                .HasColumnName("nameBackground");
        });

        modelBuilder.Entity<Lot>(entity =>
        {
            entity.HasKey(e => e.IdLot).HasName("PRIMARY");

            entity.ToTable("lots");

            entity.HasIndex(e => e.IdPresent, "fkpresLot_idx").IsUnique();

            entity.HasIndex(e => e.IdLot, "idLot_UNIQUE").IsUnique();

            entity.Property(e => e.IdLot).HasColumnName("idLot");
            entity.Property(e => e.IdPresent).HasColumnName("idPresent");
            entity.Property(e => e.IdSaler).HasColumnName("idSaler");
            entity.Property(e => e.PriceLot).HasColumnName("priceLot");
            entity.Property(e => e.StatusLot)
                .HasColumnType("enum('Active','Sold','Deleted')")
                .HasColumnName("statusLot");

            entity.HasOne(d => d.IdPresentNavigation).WithOne(p => p.Lot)
                .HasForeignKey<Lot>(d => d.IdPresent)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkpresLot");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.IdModel).HasName("PRIMARY");

            entity.ToTable("models");

            entity.Property(e => e.IdModel).HasColumnName("idModel");
            entity.Property(e => e.ImageModel)
                .HasColumnType("mediumblob")
                .HasColumnName("imageModel");
            entity.Property(e => e.NameModel)
                .HasMaxLength(45)
                .HasColumnName("nameModel");
        });

        modelBuilder.Entity<Present>(entity =>
        {
            entity.HasKey(e => e.IdPresent).HasName("PRIMARY");

            entity.ToTable("presents");

            entity.HasIndex(e => e.IdBackground, "fkBg_idx");

            entity.HasIndex(e => e.IdPresentCollection, "fkCol_idx");

            entity.HasIndex(e => e.IdModel, "fkModel_idx");

            entity.HasIndex(e => e.IdSymbol, "fkSymbol_idx");

            entity.HasIndex(e => e.IdPresent, "idPresent_UNIQUE").IsUnique();

            entity.Property(e => e.IdPresent).HasColumnName("idPresent");
            entity.Property(e => e.AuthoridPresent).HasColumnName("authoridPresent");
            entity.Property(e => e.DateUpgradePresent).HasColumnName("dateUpgradePresent");
            entity.Property(e => e.DescPresent)
                .HasColumnType("text")
                .HasColumnName("descPresent");
            entity.Property(e => e.IdBackground).HasColumnName("idBackground");
            entity.Property(e => e.IdModel).HasColumnName("idModel");
            entity.Property(e => e.IdPresentCollection).HasColumnName("idPresentCollection");
            entity.Property(e => e.IdSymbol).HasColumnName("idSymbol");
            entity.Property(e => e.ImagePresent)
                .HasColumnType("mediumblob")
                .HasColumnName("imagePresent");
            entity.Property(e => e.NumPresent).HasColumnName("numPresent");
            entity.Property(e => e.OwneridPresent).HasColumnName("owneridPresent");
            entity.Property(e => e.UpgradePresent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("upgradePresent");

            entity.HasOne(d => d.IdBackgroundNavigation).WithMany(p => p.Presents)
                .HasForeignKey(d => d.IdBackground)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkBg");

            entity.HasOne(d => d.IdModelNavigation).WithMany(p => p.Presents)
                .HasForeignKey(d => d.IdModel)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkModel");

            entity.HasOne(d => d.IdPresentCollectionNavigation).WithMany(p => p.Presents)
                .HasForeignKey(d => d.IdPresentCollection)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkCol");

            entity.HasOne(d => d.IdSymbolNavigation).WithMany(p => p.Presents)
                .HasForeignKey(d => d.IdSymbol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkSymbol");
        });

        modelBuilder.Entity<Presentcollection>(entity =>
        {
            entity.HasKey(e => e.IdPresentCollections).HasName("PRIMARY");

            entity.ToTable("presentcollections");

            entity.Property(e => e.IdPresentCollections).HasColumnName("idPresentCollections");
            entity.Property(e => e.AvailableCount).HasColumnName("availableCount");
            entity.Property(e => e.ImagePresentcollections)
                .HasColumnType("mediumblob")
                .HasColumnName("imagePresentcollections");
            entity.Property(e => e.LimitPresentCollection).HasColumnName("limitPresentCollection");
            entity.Property(e => e.NamePresentCollection)
                .HasMaxLength(70)
                .HasColumnName("namePresentCollection");

            entity.HasMany(d => d.IdModels).WithMany(p => p.IdCollections)
                .UsingEntity<Dictionary<string, object>>(
                    "CollectionModel",
                    r => r.HasOne<Model>().WithMany()
                        .HasForeignKey("IdModel")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fkModelXollection"),
                    l => l.HasOne<Presentcollection>().WithMany()
                        .HasForeignKey("IdCollection")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fkCollectionModel"),
                    j =>
                    {
                        j.HasKey("IdCollection", "IdModel")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("collection_model");
                        j.HasIndex(new[] { "IdModel" }, "fkModelXollection_idx");
                        j.IndexerProperty<int>("IdCollection").HasColumnName("idCollection");
                        j.IndexerProperty<int>("IdModel").HasColumnName("idModel");
                    });
        });

        modelBuilder.Entity<Symbol>(entity =>
        {
            entity.HasKey(e => e.IdSymbol).HasName("PRIMARY");

            entity.ToTable("symbols");

            entity.Property(e => e.IdSymbol).HasColumnName("idSymbol");
            entity.Property(e => e.ImageSymbol)
                .HasColumnType("mediumblob")
                .HasColumnName("imageSymbol");
            entity.Property(e => e.NameSymbol)
                .HasMaxLength(45)
                .HasColumnName("nameSymbol");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.IdTransaction).HasName("PRIMARY");

            entity.ToTable("transactions");

            entity.HasIndex(e => e.IdBuyer, "fkBuyerTransaction_idx");

            entity.HasIndex(e => e.IdPresent, "fkPresent_idx");

            entity.HasIndex(e => e.IdSaler, "fkSalerTransaction_idx");

            entity.Property(e => e.IdTransaction).HasColumnName("idTransaction");
            entity.Property(e => e.DateTransaction).HasColumnName("dateTransaction");
            entity.Property(e => e.IdBuyer).HasColumnName("idBuyer");
            entity.Property(e => e.IdPresent).HasColumnName("idPresent");
            entity.Property(e => e.IdSaler).HasColumnName("idSaler");
            entity.Property(e => e.SumTransaction).HasColumnName("sumTransaction");

            entity.HasOne(d => d.IdBuyerNavigation).WithMany(p => p.TransactionIdBuyerNavigations)
                .HasForeignKey(d => d.IdBuyer)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkBuyerTransaction");

            entity.HasOne(d => d.IdPresentNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.IdPresent)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkPresentTransaction");

            entity.HasOne(d => d.IdSalerNavigation).WithMany(p => p.TransactionIdSalerNavigations)
                .HasForeignKey(d => d.IdSaler)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fkSalerTransaction");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdTgUser).HasName("PRIMARY");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdTgUser, "idUser_UNIQUE").IsUnique();

            entity.HasIndex(e => e.EmailUser, "loginUser_UNIQUE").IsUnique();

            entity.Property(e => e.IdTgUser)
                .ValueGeneratedNever()
                .HasColumnName("idTgUser");
            entity.Property(e => e.BalanceUser)
                .HasDefaultValueSql("'0'")
                .HasColumnName("balanceUser");
            entity.Property(e => e.DateRegUser).HasColumnName("dateRegUser");
            entity.Property(e => e.EmailUser)
                .HasMaxLength(60)
                .HasColumnName("emailUser");
            entity.Property(e => e.NicknameUser)
                .HasMaxLength(60)
                .HasColumnName("nicknameUser");
            entity.Property(e => e.PasswordUser)
                .HasMaxLength(200)
                .HasColumnName("passwordUser");
            entity.Property(e => e.RatingUser)
                .HasDefaultValueSql("'5'")
                .HasColumnName("ratingUser");
            entity.Property(e => e.RoleUser)
                .HasDefaultValueSql("'User'")
                .HasColumnType("enum('User','Admin')")
                .HasColumnName("roleUser");

            entity.HasMany(d => d.IdPresents).WithMany(p => p.IdTgUsers)
                .UsingEntity<Dictionary<string, object>>(
                    "Cart",
                    r => r.HasOne<Lot>().WithMany()
                        .HasForeignKey("IdPresent")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fklottguser"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("IdTgUser")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("fktguser"),
                    j =>
                    {
                        j.HasKey("IdTgUser", "IdPresent")
                            .HasName("PRIMARY")
                            .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                        j.ToTable("cart");
                        j.HasIndex(new[] { "IdPresent" }, "fklot_idx");
                        j.IndexerProperty<long>("IdTgUser").HasColumnName("idTgUser");
                        j.IndexerProperty<int>("IdPresent").HasColumnName("idPresent");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
