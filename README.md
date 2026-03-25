Интернет-магазин растений "PlantShop"
Полнофункциональный интернет-магазин с административной панелью, системой заказов, корзиной, избранным и интеграцией карт. Проект реализован на ASP.NET Core MVC.

🚀 Демо
http://91.122.60.254:8080/

# Основные возможности
##Для покупателей
Каталог товаров – динамическая сортировка (по цене, названию) и поиск без перезагрузки страницы.
<img width="1902" height="901" alt="image" src="https://github.com/user-attachments/assets/9b8ebbd6-4649-4345-80de-1a2d12a15494" />


Корзина – добавление/удаление товаров, изменение количества, оформление заказа.
<img width="1197" height="566" alt="image" src="https://github.com/user-attachments/assets/dbc75923-f768-441e-b217-c6dad2933df5" />
<img width="987" height="915" alt="image" src="https://github.com/user-attachments/assets/7c5278d7-59b8-4d0c-a47a-c20a8c908b7a" />



Избранное – сохранение понравившихся товаров.
<img width="1302" height="766" alt="image" src="https://github.com/user-attachments/assets/e2f94671-a0e9-4978-8c3d-870dee1ca47c" />


Личный кабинет – просмотр профиля, истории заказов, смена пароля, редактирование данных.
<img width="1650" height="628" alt="image" src="https://github.com/user-attachments/assets/fdbacf96-07ee-41a1-96a6-d81a3aebc0f0" />


Оформление заказа – форма с адресом, телефоном, датой доставки, комментарием и интеграцией Яндекс.Карт для выбора точки на карте.


Для администратора
Управление товарами – CRUD, загрузка изображений, автоматическое создание миниатюр (300×300).

Управление категориями – древовидная структура, автогенерация slug из названия.

Управление пользователями – просмотр списка, редактирование данных, смена пароля, назначение ролей.

Управление заказами – просмотр списка, изменение статуса (Создан → В обработке → В пути → Доставлен → Отменён).
<img width="1176" height="505" alt="image" src="https://github.com/user-attachments/assets/439bd107-74c4-4545-957c-137d50924556" />
<img width="1179" height="787" alt="image" src="https://github.com/user-attachments/assets/a31578fb-7930-4b9f-a2c8-2745cccf87c9" />


🛠️ Технологии
Backend
ASP.NET Core MVC (.NET 8)

Entity Framework Core (Code First, миграции)

ASP.NET Core Identity (аутентификация, авторизация, роли)

SixLabors.ImageSharp – создание миниатюр

Serilog / ILogger – логирование

Dependency Injection – внедрение сервисов

Frontend
Bootstrap 5 + Bootstrap Icons

JavaScript (ES6) – AJAX, тосты, модальные окна, сортировка, поиск, форматирование телефона

Yandex.Maps API – выбор адреса на карте

Развёртывание
IIS (Windows Server) с .NET Hosting Bundle

Настроены права доступа к папке wwwroot/uploads

Статические файлы с кэшированием (asp-append-version)

⚙️ Установка и запуск
# Локально
## Клонировать репозиторий

### [git clone https://github.com/twixkox/OnlineShopWebApp.git](https://github.com/twixkox/OnlineShop)

### cd OnlineShopWebApp

Настроить базу данных (по умолчанию SQL Server Express)
Измените строку подключения в appsettings.json:

json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OnlineShop;Trusted_Connection=True;"
}

Применить миграции

dotnet ef database update
Запустить приложение

dotnet run
Открыть в браузере https://localhost:5001

# Тестовые данные
## Администратор:

Email: admin@gmail.com

Пароль: Admin12345!

## Пользователь - путем регистрации нового пользователя

# Контакты
### Email: twixkox@gmail.com
### Telegram: @twixkox
### GitHub: twixkox
