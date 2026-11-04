using Microsoft.EntityFrameworkCore;
using ProjectContractsPages.Models;

namespace ProjectContractsPages.Data;

/// <summary>Початкове наповнення: 4 посади, 10 працівників, 2 проекти.</summary>
public static class Seed
{
    public static void Apply(ModelBuilder b)
    {
        b.Entity<Position>().HasData(
            new Position { Id = 1, Title = "Керівник проекту", Salary = 65000 },
            new Position { Id = 2, Title = "Програміст", Salary = 52000 },
            new Position { Id = 3, Title = "Тестувальник", Salary = 38000 },
            new Position { Id = 4, Title = "Бізнес-аналітик", Salary = 45000 });

        Employee E(int id, string l, string f, string m, int pos, string addr, string ch, string ph) =>
            new() { Id = id, LastName = l, FirstName = f, MiddleName = m, PositionId = pos, Address = addr, Characteristic = ch, Phone = ph };

        b.Entity<Employee>().HasData(
            E(1, "Коваленко", "Андрій", "Петрович", 1, "м. Київ, вул. Хрещатик, 12, кв. 5", "Досвідчений керівник, відповідальний", "+380671112233"),
            E(2, "Шевченко", "Олена", "Іванівна", 2, "м. Київ, вул. Січових Стрільців, 40", "Сильна у backend-розробці", "+380502223344"),
            E(3, "Бондаренко", "Максим", "Олегович", 2, "м. Бровари, вул. Незалежності, 7", "Швидко навчається, ініціативний", "+380633334455"),
            E(4, "Ткаченко", "Ірина", "Василівна", 3, "м. Київ, просп. Перемоги, 101, кв. 14", "Уважна до деталей", "+380964445566"),
            E(5, "Кравчук", "Дмитро", "Сергійович", 4, "м. Ірпінь, вул. Університетська, 3", "Добре комунікує із замовником", "+380675556677"),
            E(6, "Мельник", "Наталія", "Андріївна", 2, "м. Київ, вул. Антоновича, 18", "Спеціаліст з frontend", "+380506667788"),
            E(7, "Олійник", "Сергій", "Миколайович", 3, "м. Вишневе, вул. Святошинська, 22", "Автоматизує тестування", "+380637778899"),
            E(8, "Поліщук", "Тетяна", "Юріївна", 4, "м. Київ, вул. Велика Васильківська, 60", "Аналітичне мислення, пунктуальна", "+380978889900"),
            E(9, "Лисенко", "Віктор", "Олександрович", 1, "м. Київ, бульв. Лесі Українки, 9", "Стратегічне планування", "+380679990011"),
            E(10, "Савченко", "Юлія", "Богданівна", 2, "м. Буча, вул. Енергетиків, 15", "Мобільна розробка, командний гравець", "+380501234567"));

        b.Entity<Project>().HasData(
            new Project { Id = 1, Name = "CRM «Клієнт+»", Customer = "ТОВ «Торгсервіс»",
                Description = "Розробка CRM-системи для обліку клієнтів, угод і звернень торгової компанії",
                StartDate = new DateTime(2026, 2, 1), EndDate = new DateTime(2026, 8, 31) },
            new Project { Id = 2, Name = "Портал «Е-Склад»", Customer = "ПП «Логістик»",
                Description = "Веб-портал складського обліку з інтеграцією зі службами доставки",
                StartDate = new DateTime(2026, 4, 15), EndDate = new DateTime(2026, 12, 20) });

        var rows = new (int Emp, int Proj, int Days)[]
        {
            (1, 1, 120), (2, 1, 150), (3, 1, 110), (4, 1, 90), (5, 1, 60), (6, 1, 100), (7, 1, 80),
            (1, 2, 95), (2, 2, 70), (3, 2, 130), (4, 2, 85), (5, 2, 75), (6, 2, 60), (8, 2, 110),
            (9, 2, 140), (10, 2, 120),
        };
        b.Entity<Participation>().HasData(rows.Select(r =>
            new Participation { EmployeeId = r.Emp, ProjectId = r.Proj, DaysWorked = r.Days }));
    }
}
