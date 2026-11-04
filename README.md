# Платформи корпоративних інформаційних систем

Миленко Юрій, група 401

| № | Тема | Папка |
|---|------|-------|
| 1 | ADO.NET. База даних «Проектні договори» | [lab1-adonet](lab1-adonet) |
| 2 | Entity Framework Core. База даних «Проектні договори» | [lab2-efcore](lab2-efcore) |
| 3 | ASP.NET Core MVC + EF Core. CRUD «Проектні договори» | [lab3-mvc](lab3-mvc) |
| 4 | ASP.NET Core Razor Pages + Web API. «Проектні договори» | [lab4-razor-api](lab4-razor-api) |
| 5 | Blazor + EF Core. CRUD «Проектні договори» | [lab5-blazor](lab5-blazor) |
| 6 | SignalR — веб-сервіс реального часу. «Проектні договори» | [lab6-signalr](lab6-signalr) |

## Запуск

Потрібен [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) (Windows для ЛР1–ЛР2, будь-яка ОС для ЛР3–ЛР5).
Окремий сервер БД не потрібен: використовується SQLite, файл бази створюється й наповнюється автоматично під час першого запуску.

```bash
cd lab1-adonet      # або lab2-efcore — застосунки Windows Forms
dotnet run

cd lab3-mvc         # або lab4-razor-api, lab5-blazor, lab6-signalr — вебзастосунки
dotnet run          # відкрийте адресу з виводу, напр. http://localhost:5000
```

Структура бази даних (ЛР1) — у файлах [lab1-adonet/Sql/schema.sql](lab1-adonet/Sql/schema.sql) та
[lab1-adonet/Sql/data.sql](lab1-adonet/Sql/data.sql). У ЛР2–ЛР5 та сама схема описана моделями EF Core
(`Models/Entities.cs`, `Data/ContractsContext.cs`, початкові дані — `Data/Seed.cs`).
