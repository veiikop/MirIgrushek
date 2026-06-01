# ООО «МирИгрушек» — информационная система

Демонстрационный экзамен. WPF (.NET 8) + Microsoft SQL Server, доступ к данным через ADO.NET.

## Структура проекта

```
MirIgrushek/
├─ MirIgrushek.sln              решение Visual Studio
├─ Database/                    Модуль 1
│  ├─ 01_Schema.sql             создание БД и таблиц (3НФ, ключи, ограничения)
│  ├─ 02_Data.sql               импорт данных из Excel-ресурсов
│  └─ ER_diagram.pdf            ER-диаграмма
└─ MirIgrushek/                 WPF-проект
   ├─ MirIgrushek.csproj
   ├─ appsettings.json          строка подключения к БД
   ├─ App.xaml(.cs)             запуск, глобальные стили (Arial, палитра)
   ├─ Models/                   сущности: Product, Order, User, Reference, UserRole
   ├─ Services/                 Database, AuthService, ProductService,
   │                            OrderService, ExcelImporter, Session
   ├─ Helpers/                  ImageHelper, Converters (правила подсветки)
   ├─ Windows/                  окна: Login, Products, ProductEdit, Orders, OrderEdit
   └─ Resources/                logo.png, icon.ico, picture.png, Images/
```

## Шаг 1. База данных

1. Открыть SSMS, подключиться к серверу.
2. Выполнить `Database/01_Schema.sql` (создаст базу `MirIgrushekDB`).
3. Выполнить `Database/02_Data.sql` (наполнит данными).

## Шаг 2. Настройка подключения

В файле `MirIgrushek/appsettings.json` указать свой сервер. Примеры:

- Локальный SQL Express:
  `Server=localhost\\SQLEXPRESS;Database=MirIgrushekDB;Trusted_Connection=True;TrustServerCertificate=True;`
- LocalDB:
  `Server=(localdb)\\MSSQLLocalDB;Database=MirIgrushekDB;Trusted_Connection=True;TrustServerCertificate=True;`

## Шаг 3. Запуск приложения

1. Открыть `MirIgrushek.sln` в Visual Studio 2022.
2. Дождаться восстановления NuGet-пакетов (Microsoft.Data.SqlClient, ClosedXML,
   Microsoft.Extensions.Configuration).
3. Нажать F5.

## Учётные записи для входа

Логины и пароли берутся из таблицы `Users`. Примеры (из импортированных данных):

| Роль          | Логин                  | Пароль |
|---------------|------------------------|--------|
| Администратор | 94d5ous@gmail.com      | uzWC67 |
| Менеджер      | 1diph5e@tutanota.com   | 8ntwUp |
| Клиент        | 5d4zbu@tutanota.com    | rwVDh9 |

Также доступен вход «как гость» (просмотр товаров без поиска/фильтра/сортировки).

## Импорт из Excel через код

Класс `Services/ExcelImporter.cs` выполняет импорт товаров напрямую из xlsx
в базу данных (метод `ImportProducts(путь_к_файлу)`), используя библиотеку
ClosedXML. Существующие товары обновляются, новые добавляются (UPSERT),
справочники создаются автоматически при отсутствии.

## Реализованный функционал по модулям

- **Модуль 2:** авторизация, роли (гость/клиент/менеджер/администратор),
  список товаров из БД с фото (заглушка при отсутствии), подсветка строк
  (нет на складе — голубой; скидка > 15% — #FFDEAD), зачёркнутая цена при
  скидке, ФИО пользователя в правом верхнем углу, логотип, иконка, выход.
- **Модуль 3:** последовательная навигация (кнопки «Назад»), обработка ошибок
  и окна сообщений, поиск/фильтр/сортировка в реальном времени (совместно),
  форма добавления/редактирования товара, удаление (запрет удаления товара
  из заказа), автогенерация артикула, ограничение фото 300×200, хранение пути
  к фото в БД, запрет открытия более одного окна редактирования.
- **Модуль 4:** кнопка «Заказы», список заказов по макету, добавление /
  редактирование / удаление заказов (только администратор).
