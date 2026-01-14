using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace DietHelper.Common.Data
{
    public class DietHelperDbContext : DbContext
    {
        //клиент
        public DietHelperDbContext()
        {
        }

        //сервер
        public DietHelperDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //клиент (сервер - через DI)
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql("Host=localhost;Database=nutrition;Username=postgres;Password=p5t9R_1g7!;Port=5432");
        }

        //старые таблицы
        public DbSet<Product> Products { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }

        //новые таблицы
        public DbSet<User> Users { get; set; }
        public DbSet<BaseProduct> BaseProducts { get; set; }
        public DbSet<UserProduct> UserProducts { get; set; }
        public DbSet<UserDish> UserDishes { get; set; }
        public DbSet<UserDishIngredient> UserDishIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //новое
            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.HasKey(u => u.Id);
            //    entity.Property(u => u.PasswordHash).IsRequired();
            //    entity.Property(u => u.Name);

            //    //entity.HasMany(u => u.Dishes)
            //    //    .WithOne(ud => ud.User)
            //    //    .HasForeignKey(ud => ud.UserId)
            //    //    .OnDelete(DeleteBehavior.Cascade);
            //});

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

                //entity.HasOne(up => up.User)
                //    .WithMany()
                //    .HasForeignKey(up => up.UserId)
                //    .OnDelete(DeleteBehavior.Cascade);

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

                //нужен отдельный индекс для BaseProductId?
                entity.HasIndex(up => new { up.UserId, up.BaseProductId }).IsUnique();
                entity.HasQueryFilter(up => !up.IsDeleted);
            });

            //Dish
            modelBuilder.Entity<UserDish>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.UserId).IsRequired();
                entity.Property(d => d.Name).IsRequired();
                entity.Property(d => d.IsReadyDish).HasDefaultValue(false);

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

            //старое
            modelBuilder.Entity<Product>().ToTable("Products", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Dish>().ToTable("Dishes", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<DishIngredient>().ToTable("DishIngredients", t => t.ExcludeFromMigrations());

            ////Product
            //modelBuilder.Entity<Product>(entity =>
            //{
            //    entity.HasKey(p => p.Id);
            //    entity.Property(p => p.Name).IsRequired();

            //    //NutritionInfo
            //    entity.OwnsOne(p => p.NutritionFacts, nutrition =>
            //    {
            //        nutrition.Property(n => n.Calories).HasColumnName("Calories");
            //        nutrition.Property(n => n.Protein).HasColumnName("Protein");
            //        nutrition.Property(n => n.Fat).HasColumnName("Fat");
            //        nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
            //    });

            //    //связь с DishIngredients
            //    entity.HasMany(p => p.DishIngredients)
            //    .WithOne(di => di.Ingredient)
            //    .HasForeignKey(di => di.ProductId)
            //    .OnDelete(DeleteBehavior.Restrict);
            //});

            ////Dish
            //modelBuilder.Entity<Dish>(entity =>
            //{
            //    entity.HasKey(d => d.Id);
            //    entity.Property(d => d.Name).IsRequired();

            //    //NutritionInfo
            //    entity.OwnsOne(d => d.NutritionFacts, nutrition =>
            //    {
            //        nutrition.Property(n => n.Calories).HasColumnName("Calories");
            //        nutrition.Property(n => n.Protein).HasColumnName("Protein");
            //        nutrition.Property(n => n.Fat).HasColumnName("Fat");
            //        nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
            //    });

            //    //связь с DishIngredients
            //    entity.HasMany(d => d.Ingredients)
            //    .WithOne(di => di.Dish)
            //    .HasForeignKey(di => di.DishId)
            //    .OnDelete(DeleteBehavior.Cascade);
            //});

            ////DishIngredient
            //modelBuilder.Entity<DishIngredient>(entity =>
            //{
            //    entity.HasKey(di => di.Id);

            //    entity.Property(di => di.Quantity).IsRequired();

            //    //убрать дублирование
            //    entity.HasIndex(di => new { di.DishId, di.ProductId }).IsUnique();
            //});
        }
    }
}
