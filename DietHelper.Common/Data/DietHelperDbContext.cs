using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.MealEntries;
using DietHelper.Common.Models.Products;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DietHelper.Common.Data
{
    public class DietHelperDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        //клиент
        public DietHelperDbContext()
        {
        }

        //сервер
        public DietHelperDbContext(DbContextOptions<DietHelperDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //клиент (сервер - через DI)
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql("Host=localhost;Database=nutrition;Username=postgres;Password=p5t9R_1g7!;Port=5432");
        }

        //новые таблицы
        public DbSet<BaseProduct> BaseProducts { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }
        public DbSet<UserDish> UserDishes { get; set; }
        public DbSet<UserDishIngredient> UserDishIngredients { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserMealEntry> UserMealEntries { get; set; }
        public DbSet<UserMealEntryIngredient> UserMealEntryIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                    .WithMany()
                    .HasForeignKey(rt => rt.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.Token).IsUnique();

                entity.HasIndex(rt => rt.UserId);
            });

            modelBuilder.Entity<BaseProduct>(entity =>
            {
                entity.HasKey(bp => bp.Id);
                entity.Property(bp => bp.Name).IsRequired();

                entity.OwnsOne(bp => bp.NutritionFacts, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("Calories");
                    nutrition.Property(n => n.Protein).HasColumnName("Protein");
                    nutrition.Property(n => n.Fat).HasColumnName("Fat");
                    nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
                });

                entity.HasIndex(bp => bp.Name);
                entity.HasQueryFilter(bp => !bp.IsDeleted);
            });

            modelBuilder.Entity<UserProduct>(entity =>
            {
                entity.HasKey(up => up.Id);
                entity.Property(up => up.UserId);
                entity.Property(up => up.BaseProductId);

                entity.HasOne(up => up.User)
                    .WithMany()
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(up => up.UserId);

                entity.HasOne(up => up.BaseProduct)
                    .WithMany()
                    .HasForeignKey(up => up.BaseProductId)
                    .OnDelete(DeleteBehavior.SetNull);


                entity.OwnsOne(up => up.CustomNutrition, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("CustomCalories");
                    nutrition.Property(n => n.Protein).HasColumnName("CustomProtein");
                    nutrition.Property(n => n.Fat).HasColumnName("CustomFat");
                    nutrition.Property(n => n.Carbs).HasColumnName("CustomCarbs");
                });

                entity.HasIndex(up => new { up.UserId, up.BaseProductId }).IsUnique();
                entity.HasQueryFilter(up => !up.IsDeleted);
            });

            modelBuilder.Entity<UserDish>(entity =>
            {
                entity.HasKey(ud => ud.Id);
                entity.Property(ud => ud.UserId).IsRequired();
                entity.Property(ud => ud.Name).IsRequired();
                entity.Property(ud => ud.IsReadyDish).HasDefaultValue(false);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(ud => ud.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.OwnsOne(ud => ud.NutritionFacts, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("Calories");
                    nutrition.Property(n => n.Protein).HasColumnName("Protein");
                    nutrition.Property(n => n.Fat).HasColumnName("Fat");
                    nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
                });

                entity.HasQueryFilter(d => !d.IsDeleted);
            });

            modelBuilder.Entity<UserDishIngredient>(entity =>
            {
                entity.HasKey(udi => udi.Id);
                entity.Property(udi => udi.UserProductId).IsRequired();
                entity.Property(udi => udi.UserDishId).IsRequired();
                entity.Property(udi => udi.Quantity).IsRequired();

                entity.HasOne<UserDish>()
                    .WithMany(ud => ud.Ingredients)
                    .HasForeignKey(udi => udi.UserDishId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(udi => udi.UserProduct)
                    .WithMany()
                    .HasForeignKey(udi => udi.UserProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(udi => new { udi.UserDishId, udi.UserProductId }).IsUnique();

                entity.HasQueryFilter(udi => !udi.IsDeleted);
            });

            modelBuilder.Entity<UserMealEntry>(entity =>
            {
                entity.HasKey(ume => ume.Id);
                entity.Property(ume => ume.Date).IsRequired();
                entity.Property(ume => ume.Date).HasColumnType("date");
                entity.Property(ume => ume.CreatedAt).HasColumnType("timestamp with time zone");

                entity.OwnsOne(ume => ume.TotalNutrition, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("TotalCalories");
                    nutrition.Property(n => n.Protein).HasColumnName("TotalProtein");
                    nutrition.Property(n => n.Fat).HasColumnName("TotalFat");
                    nutrition.Property(n => n.Carbs).HasColumnName("TotalCarbs");
                });

                entity.HasOne(ume => ume.User)
                    .WithMany()
                    .HasForeignKey(ume => ume.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ume => ume.UserDish)
                    .WithMany()
                    .HasForeignKey(ume => ume.UserDishId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(ume => new { ume.UserId, ume.Date });
            });

            modelBuilder.Entity<UserMealEntryIngredient>(entity =>
            {
                entity.HasKey(umei => umei.Id);
                entity.Property(umei => umei.ProductNameSnapshot).IsRequired();

                entity.OwnsOne(umei => umei.ProductNutritionInfoSnapshot, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("CaloriesSnapshot");
                    nutrition.Property(n => n.Protein).HasColumnName("ProteinSnapshot");
                    nutrition.Property(n => n.Fat).HasColumnName("FatSnapshot");
                    nutrition.Property(n => n.Carbs).HasColumnName("CarbsSnapshot");
                });

                entity.HasOne(umei => umei.UserMealEntry)
                    .WithMany(umei => umei.Ingredients)
                    .HasForeignKey(umei => umei.UserMealEntryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(umei => umei.UserProduct)
                    .WithMany()
                    .HasForeignKey(umei => umei.UserProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(umei => umei.UserMealEntryId);
            });
        }
    }
}
