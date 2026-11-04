namespace ProjectContracts;

/// <summary>SQL-запити, які відображає додаток.</summary>
public static class Queries
{
    public const string Employees = @"
SELECT e.Id AS '№',
       e.LastName || ' ' || e.FirstName || ' ' || e.MiddleName AS 'ПІБ',
       p.Title  AS 'Посада',
       e.Address AS 'Домашня адреса',
       e.Phone  AS 'Телефон',
       e.Characteristic AS 'Характеристика'
FROM Employees e JOIN Positions p ON p.Id = e.PositionId
ORDER BY e.LastName";

    public const string Positions = @"
SELECT p.Id AS '№', p.Title AS 'Посада', printf('%.2f', p.Salary) AS 'Оклад, грн',
       COUNT(e.Id) AS 'Працівників'
FROM Positions p LEFT JOIN Employees e ON e.PositionId = p.Id
GROUP BY p.Id ORDER BY p.Salary DESC";

    public const string Projects = @"
SELECT pr.Id AS '№', pr.Name AS 'Назва проекту', pr.Customer AS 'Замовник',
       strftime('%d.%m.%Y', pr.StartDate) AS 'Початок',
       strftime('%d.%m.%Y', pr.EndDate)   AS 'Закінчення',
       CAST(julianday(pr.EndDate) - julianday(pr.StartDate) AS INTEGER) AS 'Тривалість, дн.',
       COUNT(pa.EmployeeId) AS 'Учасників',
       pr.Description AS 'Опис проекту'
FROM Projects pr LEFT JOIN Participation pa ON pa.ProjectId = pr.Id
GROUP BY pr.Id ORDER BY pr.StartDate";

    public const string Participation = @"
SELECT pr.Name AS 'Проект',
       e.LastName || ' ' || substr(e.FirstName,1,1) || '. ' || substr(e.MiddleName,1,1) || '.' AS 'Працівник',
       p.Title AS 'Посада',
       pa.DaysWorked AS 'Днів у проекті',
       printf('%.2f', p.Salary) AS 'Оклад, грн'
FROM Participation pa
JOIN Employees e  ON e.Id  = pa.EmployeeId
JOIN Positions p  ON p.Id  = e.PositionId
JOIN Projects  pr ON pr.Id = pa.ProjectId
WHERE @projectId = 0 OR pa.ProjectId = @projectId
ORDER BY pr.Name, e.LastName";

    public const string TwoProjects = @"
SELECT e.LastName || ' ' || e.FirstName || ' ' || e.MiddleName AS 'ПІБ',
       p.Title AS 'Посада',
       COUNT(pa.ProjectId) AS 'Проектів',
       SUM(pa.DaysWorked)  AS 'Усього днів',
       group_concat(pr.Name, '; ') AS 'Проекти'
FROM Employees e
JOIN Positions p ON p.Id = e.PositionId
JOIN Participation pa ON pa.EmployeeId = e.Id
JOIN Projects pr ON pr.Id = pa.ProjectId
GROUP BY e.Id HAVING COUNT(pa.ProjectId) >= 2
ORDER BY e.LastName";

    public const string Search = @"
SELECT e.LastName || ' ' || e.FirstName || ' ' || e.MiddleName AS 'ПІБ',
       p.Title AS 'Посада', e.Address AS 'Домашня адреса',
       IFNULL(group_concat(pr.Name || ' (' || pa.DaysWorked || ' дн.)', '; '), '—') AS 'Участь у проектах'
FROM Employees e
JOIN Positions p ON p.Id = e.PositionId
LEFT JOIN Participation pa ON pa.EmployeeId = e.Id
LEFT JOIN Projects pr ON pr.Id = pa.ProjectId
WHERE e.LastName LIKE @pattern
GROUP BY e.Id ORDER BY e.LastName";

    public const string ProjectList = "SELECT Id, Name FROM Projects ORDER BY Name";
}
