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

        public DbSet<Product> Products { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DishIngredient> DishIngredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired();

                //NutritionInfo
                entity.OwnsOne(p => p.NutritionFacts, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("Calories");
                    nutrition.Property(n => n.Protein).HasColumnName("Protein");
                    nutrition.Property(n => n.Fat).HasColumnName("Fat");
                    nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
                });

                //связь с DishIngredients
                entity.HasMany(p => p.DishIngredients)
                .WithOne(di => di.Ingredient)
                .HasForeignKey(di => di.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            //Dish
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired();

                //NutritionInfo
                entity.OwnsOne(d => d.NutritionFacts, nutrition =>
                {
                    nutrition.Property(n => n.Calories).HasColumnName("Calories");
                    nutrition.Property(n => n.Protein).HasColumnName("Protein");
                    nutrition.Property(n => n.Fat).HasColumnName("Fat");
                    nutrition.Property(n => n.Carbs).HasColumnName("Carbs");
                });

                //связь с DishIngredients
                entity.HasMany(d => d.Ingredients)
                .WithOne(di => di.Dish)
                .HasForeignKey(di => di.DishId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            //DishIngredient
            modelBuilder.Entity<DishIngredient>(entity =>
            {
                entity.HasKey(di => di.Id);

                entity.Property(di => di.Quantity).IsRequired();

                //убрать дублирование
                entity.HasIndex(di => new { di.DishId, di.ProductId }).IsUnique();
            });
        }
    }
}
