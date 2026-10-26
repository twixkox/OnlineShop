using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OnlineShop.Db.Models;

namespace OnlineShop.Db
{
    public static class ShopDbInitilizer
    {
        public static async Task InitializeAsync(DatabaseContext context, ILogger? logger = null)
        {
            logger ??= NullLogger.Instance;
            if (!await context.Categories.AnyAsync() || !await context.Products.AnyAsync())
            {
                logger.LogInformation("Первый запуск: заполняем базу тестовыми категориями и товарами");

                var categories = await CreateCategoriesAsync(context);
                var productsCount = await CreateProductsAsync(context, categories);
                logger.LogInformation("Создано категорий: {CategoriesCount}, товаров: {ProductsCount}", categories.Count, productsCount);
            }

            // Для уже существующих баз: дополняем характеристики стандартных товаров и создаём промокоды
            await EnrichKnownProductsAsync(context);
            await CreatePromoCodesAsync(context);
        }

        private record PlantInfo(string Height, int? Zone, LightRequirement? Light, WateringLevel? Watering, string? Period, int Stock);

        // Характеристики стандартных товаров каталога (по началу названия)
        private static readonly Dictionary<string, PlantInfo> KnownPlants = new()
        {
            ["Яблоня 'Антоновка'"] = new("до 6 м", 3, LightRequirement.Sun, WateringLevel.Medium, "сентябрь–октябрь", 12),
            ["Груша 'Конференция'"] = new("до 5 м", 5, LightRequirement.Sun, WateringLevel.Medium, "сентябрь", 8),
            ["Смородина черная 'Добрыня'"] = new("до 1,7 м", 3, LightRequirement.Sun, WateringLevel.High, "июль", 25),
            ["Клен остролистный 'Globosum'"] = new("5–6 м", 4, LightRequirement.Sun, WateringLevel.Medium, null, 5),
            ["Береза повислая 'Youngii'"] = new("до 4 м", 2, LightRequirement.Sun, WateringLevel.Medium, null, 4),
            ["Гортензия древовидная 'Annabelle'"] = new("1–1,5 м", 3, LightRequirement.PartShade, WateringLevel.High, "июль–сентябрь", 18),
            ["Ель голубая 'Hoopsii'"] = new("10–15 м", 2, LightRequirement.Sun, WateringLevel.Low, null, 3),
            ["Сосна горная 'Mughus'"] = new("2–3 м", 2, LightRequirement.Sun, WateringLevel.Low, null, 7),
            ["Туя западная 'Smaragd'"] = new("4–5 м", 4, LightRequirement.PartShade, WateringLevel.Medium, null, 30),
            ["Петуния каскадная 'Рамблин'"] = new("до 0,4 м", null, LightRequirement.Sun, WateringLevel.High, "июнь–сентябрь", 60),
            ["Бархатцы отклоненные 'Бонита'"] = new("до 0,3 м", null, LightRequirement.Sun, WateringLevel.Low, "июнь–октябрь", 80),
            ["Цинния изящная 'Дримленд'"] = new("до 0,3 м", null, LightRequirement.Sun, WateringLevel.Medium, "июль–сентябрь", 50),
            ["Пион молочноцветковый 'Sarah Bernhardt'"] = new("0,8–1 м", 3, LightRequirement.Sun, WateringLevel.Medium, "июнь", 15),
            ["Лилия азиатская 'Tiny Diamond'"] = new("до 0,4 м", 4, LightRequirement.Sun, WateringLevel.Medium, "июнь–июль", 40),
            ["Хоста гибридная 'Blue Angel'"] = new("до 0,9 м", 3, LightRequirement.Shade, WateringLevel.High, "июль–август", 20),
            ["Роза чайно-гибридная 'Black Magic'"] = new("1–1,2 м", 6, LightRequirement.Sun, WateringLevel.Medium, "июнь–сентябрь", 0),
            ["Роза флорибунда 'Leonardo da Vinci'"] = new("0,8–1 м", 5, LightRequirement.Sun, WateringLevel.Medium, "июнь–сентябрь", 14),
            ["Роза плетистая 'Flammentanz'"] = new("3–5 м", 4, LightRequirement.Sun, WateringLevel.Medium, "июнь–июль", 9),
            ["Грунт универсальный 10л"] = new("", null, null, null, null, 100),
            ["Удобрение для роз 1кг"] = new("", null, null, null, null, 45),
            ["Секатор профессиональный"] = new("", null, null, null, null, 12),
        };

        private static async Task EnrichKnownProductsAsync(DatabaseContext context)
        {
            var products = await context.Products.ToListAsync();
            var changed = false;

            foreach (var product in products)
            {
                if (!KnownPlants.TryGetValue(product.Name, out var info)) continue;
                if (product.Height != null || product.Light != null || product.FrostZone != null) continue;

                product.Height = string.IsNullOrEmpty(info.Height) ? null : info.Height;
                product.FrostZone = info.Zone;
                product.Light = info.Light;
                product.Watering = info.Watering;
                product.FloweringPeriod = info.Period;
                product.Stock = info.Stock;
                changed = true;
            }

            if (changed) await context.SaveChangesAsync();
        }

        private static async Task CreatePromoCodesAsync(DatabaseContext context)
        {
            if (await context.PromoCodes.AnyAsync()) return;

            context.PromoCodes.AddRange(
                new PromoCode { Code = "SPRING10", DiscountPercent = 10, Description = "Весенняя скидка 10% на любой заказ" },
                new PromoCode { Code = "GARDEN15", DiscountPercent = 15, MinOrderAmount = 5000, Description = "15% на заказ от 5 000 ₽" });

            await context.SaveChangesAsync();
        }

        private static async Task<Dictionary<string, Category>> CreateCategoriesAsync(DatabaseContext context)
        {
            var categories = new Dictionary<string, Category>();

            var rootCategories = new List<Category>
        {
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Плодовые",
                IdentityUrl = "plodovye",
                Description = "Плодовые деревья и кустарники для сада",
                PhotoPath = "uploads/category/anyCategory.png"

            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Лиственные",
                IdentityUrl = "listvennye",
                Description = "Декоративно-лиственные деревья и кустарники",
                PhotoPath = "uploads/category/anyCategory.png"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Хвойные",
                IdentityUrl = "hvoinye",
                Description = "Вечнозелёные хвойные растения",
                PhotoPath = "uploads/category/anyCategory.png"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Однолетники",
                IdentityUrl = "odnoletniki",
                Description = "Цветущие однолетние растения",
                PhotoPath = "uploads/category/anyCategory.png"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Многолетники",
                IdentityUrl = "mnogoletniki",
                Description = "Многолетние цветы и декоративные растения",
                PhotoPath = "uploads/category/anyCategory.png"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Розы",
                IdentityUrl = "rozy",
                Description = "Садовые розы всех сортов",
                PhotoPath = "uploads/category/anyCategory.png"
            },
            new Category
            {
                Id = Guid.NewGuid(),
                Name = "Сопутствующие товары",
                IdentityUrl = "soputstvuyushchie-tovary",
                Description = "Всё для ухода за растениями",
                PhotoPath = "uploads/category/anyCategory.png"
            }
        };

            context.Categories.AddRange(rootCategories);
            await context.SaveChangesAsync();

            foreach (var cat in rootCategories)
            {
                categories[cat.IdentityUrl] = cat;
            }

            return categories;
        }

        private static async Task<int> CreateProductsAsync(DatabaseContext context, Dictionary<string, Category> categories)
        {
            var products = new List<Product>
    {
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Яблоня 'Антоновка'",
            Description = "Классический сорт яблони с ароматными плодами. Срок созревания: сентябрь-октябрь.",
            Cost = 890.00m,
            CategoryId = categories["plodovye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Груша 'Конференция'",
            Description = "Популярный сорт груши с сочными сладкими плодами. Устойчива к болезням.",
            Cost = 950.00m,
            CategoryId = categories["plodovye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Смородина черная 'Добрыня'",
            Description = "Крупноплодный сорт черной смородины. Ягоды сладкие, ароматные.",
            Cost = 450.00m,
            CategoryId = categories["plodovye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Клен остролистный 'Globosum'",
            Description = "Декоративный клен с шаровидной кроной. Высота до 5-6 м.",
            Cost = 3500.00m,
            CategoryId = categories["listvennye"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Береза повислая 'Youngii'",
            Description = "Плакучая форма березы на штамбе. Очень декоративна.",
            Cost = 4200.00m,
            CategoryId = categories["listvennye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Гортензия древовидная 'Annabelle'",
            Description = "Роскошный кустарник с огромными белыми соцветиями.",
            Cost = 890.00m,
            CategoryId = categories["listvennye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Ель голубая 'Hoopsii'",
            Description = "Эффектная ель с голубой хвоей. Высота до 10-15 м.",
            Cost = 5600.00m,
            CategoryId = categories["hvoinye"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Сосна горная 'Mughus'",
            Description = "Неприхотливая сосна шаровидной формы. Высота до 2-3 м.",
            Cost = 2900.00m,
            CategoryId = categories["hvoinye"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Туя западная 'Smaragd'",
            Description = "Популярный сорт туи с изумрудной хвоей. Идеальна для живой изгороди.",
            Cost = 1800.00m,
            CategoryId = categories["hvoinye"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Петуния каскадная 'Рамблин'",
            Description = "Обильноцветущая петуния для кашпо и балконных ящиков.",
            Cost = 150.00m,
            CategoryId = categories["odnoletniki"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Бархатцы отклоненные 'Бонита'",
            Description = "Низкорослые бархатцы с махровыми соцветиями.",
            Cost = 80.00m,
            CategoryId = categories["odnoletniki"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Цинния изящная 'Дримленд'",
            Description = "Цинния с крупными махровыми соцветиями ярких расцветок.",
            Cost = 120.00m,
            CategoryId = categories["odnoletniki"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Пион молочноцветковый 'Sarah Bernhardt'",
            Description = "Классический пион с розовыми махровыми цветами.",
            Cost = 890.00m,
            CategoryId = categories["mnogoletniki"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Лилия азиатская 'Tiny Diamond'",
            Description = "Низкорослая лилия с ярко-желтыми цветами.",
            Cost = 320.00m,
            CategoryId = categories["mnogoletniki"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Хоста гибридная 'Blue Angel'",
            Description = "Крупная хоста с голубоватыми листьями.",
            Cost = 650.00m,
            CategoryId = categories["mnogoletniki"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Роза чайно-гибридная 'Black Magic'",
            Description = "Роза с бархатистыми темно-красными цветами.",
            Cost = 890.00m,
            CategoryId = categories["rozy"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Роза флорибунда 'Leonardo da Vinci'",
            Description = "Обильноцветущая роза с розовыми махровыми цветами.",
            Cost = 950.00m,
            CategoryId = categories["rozy"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Роза плетистая 'Flammentanz'",
            Description = "Мощная плетистая роза с красными махровыми цветами.",
            Cost = 1200.00m,
            CategoryId = categories["rozy"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Грунт универсальный 10л",
            Description = "Питательный грунт для всех видов растений.",
            Cost = 250.00m,
            CategoryId = categories["soputstvuyushchie-tovary"].Id,
            PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Удобрение для роз 1кг",
            Description = "Специализированное удобрение для роз пролонгированного действия.",
            Cost = 380.00m,
            CategoryId = categories["soputstvuyushchie-tovary"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        },
        new Product
        {
            Id = Guid.NewGuid(),
            Name = "Секатор профессиональный",
            Description = "Острый секатор для обрезки растений.",
            Cost = 1200.00m,
            CategoryId = categories["soputstvuyushchie-tovary"].Id,
           PhotoPath = "uploads/products/original/anyProduct.png",
            ThumbnailPath = "uploads/products/original/anyProduct.png"
        }
    };
            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            return products.Count;
        }
    }
}
