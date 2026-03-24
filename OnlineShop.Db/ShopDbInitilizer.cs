using Microsoft.EntityFrameworkCore;
using OnlineShop.Db.Models;

namespace OnlineShop.Db
{
    public static class ShopDbInitilizer
    {
        public static async Task InitializeAsync(DatabaseContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.Categories.AnyAsync() && await context.Products.AnyAsync())
            {
                Console.WriteLine("База данных уже заполнена");
                return;
            }

            Console.WriteLine("Инициализация базы данных магазина растений...");

            var categories = await CreateCategoriesAsync(context);
            await CreateProductsAsync(context, categories);
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

            Console.WriteLine($" Создано категорий: {categories.Count}");
            return categories;
        }

        private static async Task CreateProductsAsync(DatabaseContext context, Dictionary<string, Category> categories)
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

            Console.WriteLine($" Создано продуктов: {products.Count}");
        }
    }
}
