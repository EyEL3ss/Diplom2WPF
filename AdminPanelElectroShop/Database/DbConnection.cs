using AdminPanelElectroShop.Classes;
using AdminPanelElectroShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdminPanelElectroShop.Database
{
    public class DbConnection : DbContext
    {
        public const string ConnectionString =
            "Server=141.8.192.186;Port=3306;Database=a1215492_ElectroBase;Uid=a1215492_ElectroBase;Pwd=2281337992cfa;SslMode=None;ConnectionTimeout=15;DefaultCommandTimeout=30;";

        public DbConnection(DbContextOptions<DbConnection> options) : base(options)
        {
        }

        // ✅ Пустой конструктор (нужен для миграций и OnConfiguring)
        public DbConnection()
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductSpecification> ProductSpecifications { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<PromoCode> PromoCodes { get; set; }
        public DbSet<SellerApplication> SellerApplications { get; set; }
        public DbSet<ProductDiscount> ProductDiscounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            optionsBuilder.UseMySql(
                ConnectionString,
                new MySqlServerVersion(new Version(8, 0, 21)),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Classes.User>().ToTable("Users");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");
            modelBuilder.Entity<ProductSpecification>().ToTable("ProductSpecifications");
            modelBuilder.Entity<ProductVariant>().ToTable("ProductVariants");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<WishlistItem>().ToTable("Wishlist");
            modelBuilder.Entity<Classes.Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<Review>().ToTable("Reviews");
            modelBuilder.Entity<Notifications>().ToTable("Notifications");
            modelBuilder.Entity<PromoCode>().ToTable("PromoCodes");
            modelBuilder.Entity<SellerApplication>().ToTable("SellerApplications");
            modelBuilder.Entity<ProductDiscount>().ToTable("ProductDiscounts");

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.CreatedAt)
                .HasColumnName("added_at");

            modelBuilder.Entity<WishlistItem>()
                .Property(wi => wi.CreatedAt)
                .HasColumnName("added_at");

            modelBuilder.Entity<CartItem>().Ignore(ci => ci.UpdatedAt);
            modelBuilder.Entity<WishlistItem>().Ignore(wi => wi.UpdatedAt);
            modelBuilder.Entity<Category>().Ignore(c => c.UpdatedAt);
            modelBuilder.Entity<Notifications>().Ignore(n => n.UpdatedAt);
            modelBuilder.Entity<Review>().Ignore(r => r.UpdatedAt);
            modelBuilder.Entity<ProductImage>().Ignore(pi => pi.CreatedAt);
            modelBuilder.Entity<ProductImage>().Ignore(pi => pi.UpdatedAt);
            modelBuilder.Entity<ProductSpecification>().Ignore(ps => ps.CreatedAt);
            modelBuilder.Entity<ProductSpecification>().Ignore(ps => ps.UpdatedAt);
            modelBuilder.Entity<ProductVariant>().Ignore(pv => pv.CreatedAt);
            modelBuilder.Entity<ProductVariant>().Ignore(pv => pv.UpdatedAt);
            modelBuilder.Entity<OrderItem>().Ignore(oi => oi.CreatedAt);
            modelBuilder.Entity<OrderItem>().Ignore(oi => oi.UpdatedAt);
            modelBuilder.Entity<PromoCode>().Ignore(pc => pc.CreatedAt);
            modelBuilder.Entity<PromoCode>().Ignore(pc => pc.UpdatedAt);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("idx_user_email");

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.PasswordHash)
           .HasColumnName("password_hash")
                   .IsRequired()
                    .HasMaxLength(255);
            });

            modelBuilder.Entity<User>()
            .HasIndex(u => u.Phone)
            .IsUnique()
            .HasDatabaseName("idx_user_phone");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Role)
                .HasDatabaseName("idx_user_role");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique()
                .HasDatabaseName("idx_category_slug");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .HasDatabaseName("idx_category_name");

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.ParentId)
                .HasDatabaseName("idx_category_parent");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Name)
                .HasDatabaseName("idx_product_name");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Brand)
                .HasDatabaseName("idx_product_brand");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price)
                .HasDatabaseName("idx_product_price");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Rating)
                .HasDatabaseName("idx_product_rating");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId)
                .HasDatabaseName("idx_product_category");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Status)
                .HasDatabaseName("idx_product_status");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsNew)
                .HasDatabaseName("idx_product_new");

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.IsHit)
                .HasDatabaseName("idx_product_hit");

            modelBuilder.Entity<ProductImage>()
                .HasIndex(pi => pi.ProductId)
                .HasDatabaseName("idx_productimage_product");

            modelBuilder.Entity<ProductSpecification>()
                .HasIndex(ps => ps.ProductId)
                .HasDatabaseName("idx_pspec_product");

            modelBuilder.Entity<ProductSpecification>()
                .HasIndex(ps => ps.SpecKey)
                .HasDatabaseName("idx_pspec_key");

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.ProductId)
                .HasDatabaseName("idx_variant_product");

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.Color)
                .HasDatabaseName("idx_variant_color");

            modelBuilder.Entity<ProductVariant>()
                .HasIndex(pv => pv.Storage)
                .HasDatabaseName("idx_variant_storage");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.UserId)
                .HasDatabaseName("idx_cart_user");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.ProductId)
                .HasDatabaseName("idx_cart_product");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => ci.VariantId)
                .HasDatabaseName("idx_cart_variant");

            modelBuilder.Entity<CartItem>()
                .HasIndex(ci => new { ci.UserId, ci.ProductId, ci.VariantId })
                .IsUnique()
                .HasDatabaseName("idx_cart_unique");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(wi => wi.UserId)
                .HasDatabaseName("idx_wishlist_user");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(wi => wi.ProductId)
                .HasDatabaseName("idx_wishlist_product");

            modelBuilder.Entity<WishlistItem>()
                .HasIndex(wi => new { wi.UserId, wi.ProductId })
                .IsUnique()
                .HasDatabaseName("idx_wishlist_unique");

            modelBuilder.Entity<Classes.Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique()
                .HasDatabaseName("idx_order_number");

            modelBuilder.Entity<Classes.Order>()
                .HasIndex(o => o.UserId)
                .HasDatabaseName("idx_order_user");

            modelBuilder.Entity<Classes.Order>()
                .HasIndex(o => o.Status)
                .HasDatabaseName("idx_order_status");

            modelBuilder.Entity<Classes.Order>()
                .HasIndex(o => o.CreatedAt)
                .HasDatabaseName("idx_order_date");

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.OrderId)
                .HasDatabaseName("idx_orderitem_order");

            modelBuilder.Entity<OrderItem>()
                .HasIndex(oi => oi.ProductId)
                .HasDatabaseName("idx_orderitem_product");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.ProductId)
                .HasDatabaseName("idx_review_product");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.UserId)
                .HasDatabaseName("idx_review_user");

            modelBuilder.Entity<Review>()
                .HasIndex(r => r.Rating)
                .HasDatabaseName("idx_review_rating");

            modelBuilder.Entity<Notifications>()
                .HasIndex(n => n.UserId)
                .HasDatabaseName("idx_notification_user");

            modelBuilder.Entity<Notifications>()
                .HasIndex(n => n.IsRead)
                .HasDatabaseName("idx_notification_read");

            modelBuilder.Entity<PromoCode>()
                .HasIndex(pc => pc.Code)
                .IsUnique()
                .HasDatabaseName("idx_promo_code");

            modelBuilder.Entity<PromoCode>()
                .HasIndex(pc => pc.IsActive)
                .HasDatabaseName("idx_promo_active");

            modelBuilder.Entity<PromoCode>()
                .HasIndex(pc => pc.UserId)
                .HasDatabaseName("idx_promo_user");

            modelBuilder.Entity<ProductDiscount>()
                .HasIndex(pd => pd.ProductId)
                .HasDatabaseName("idx_product_discount_product");

            modelBuilder.Entity<ProductDiscount>()
                .HasIndex(pd => pd.IsActive)
                .HasDatabaseName("idx_product_discount_active");

            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductSpecification>()
                .HasOne(ps => ps.Product)
                .WithMany(p => p.Specifications)
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductVariant>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(ci => ci.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Variant)
                .WithMany(pv => pv.CartItems)
                .HasForeignKey(ci => ci.VariantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.User)
                .WithMany(u => u.WishlistItems)
                .HasForeignKey(wi => wi.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany(p => p.WishlistItems)
                .HasForeignKey(wi => wi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Classes.Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notifications>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PromoCode>()
                .HasOne(pc => pc.User)
                .WithMany(u => u.PromoCodes)
                .HasForeignKey(pc => pc.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ProductDiscount>()
                .HasOne(pd => pd.Product)
                .WithMany(p => p.Discounts)
                .HasForeignKey(pd => pd.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            ApplySnakeCaseColumnNames(modelBuilder);
        }

        private static void ApplySnakeCaseColumnNames(ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var tableName = entity.GetTableName();
                if (string.IsNullOrWhiteSpace(tableName))
                {
                    continue;
                }

                var tableIdentifier = StoreObjectIdentifier.Table(tableName, entity.GetSchema());

                foreach (var property in entity.GetProperties())
                {
                    var columnName = property.GetColumnName(tableIdentifier);
                    if (!string.IsNullOrWhiteSpace(columnName))
                    {
                        property.SetColumnName(ToSnakeCase(columnName));
                    }
                }

                foreach (var index in entity.GetIndexes())
                {
                    var databaseName = index.GetDatabaseName();
                    if (!string.IsNullOrWhiteSpace(databaseName))
                    {
                        index.SetDatabaseName(ToSnakeCase(databaseName));
                    }
                }
            }
        }

        private static string ToSnakeCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var builder = new StringBuilder(value.Length + 8);

            for (var i = 0; i < value.Length; i++)
            {
                var currentChar = value[i];

                if (char.IsUpper(currentChar))
                {
                    var hasPrevious = i > 0;
                    var hasNext = i + 1 < value.Length;

                    if (hasPrevious &&
                        (char.IsLower(value[i - 1]) ||
                         char.IsDigit(value[i - 1]) ||
                         (hasNext && char.IsLower(value[i + 1]))))
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(currentChar));
                }
                else
                {
                    builder.Append(currentChar);
                }
            }

            return builder.ToString();
        }
    }
}
