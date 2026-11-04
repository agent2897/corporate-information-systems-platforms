-- База даних «Проектні договори» (3НФ)
PRAGMA foreign_keys = ON;

DROP TABLE IF EXISTS Participation;
DROP TABLE IF EXISTS Employees;
DROP TABLE IF EXISTS Projects;
DROP TABLE IF EXISTS Positions;

-- Посади та посадові оклади
CREATE TABLE Positions (
    Id      INTEGER PRIMARY KEY AUTOINCREMENT,
    Title   TEXT    NOT NULL UNIQUE,
    Salary  REAL    NOT NULL CHECK (Salary > 0)
);

-- Працівники
CREATE TABLE Employees (
    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
    LastName       TEXT    NOT NULL,
    FirstName      TEXT    NOT NULL,
    MiddleName     TEXT    NOT NULL,
    PositionId     INTEGER NOT NULL REFERENCES Positions(Id),
    Address        TEXT    NOT NULL,
    Characteristic TEXT,
    Phone          TEXT              -- додаткове поле
);

-- Проекти
CREATE TABLE Projects (
    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
    Name        TEXT NOT NULL UNIQUE,
    Description TEXT,
    StartDate   TEXT NOT NULL,
    EndDate     TEXT NOT NULL,
    Customer    TEXT,                 -- додаткове поле
    CHECK (EndDate >= StartDate)
);

-- Участь працівників у проектах (зв'язок M:N)
CREATE TABLE Participation (
    EmployeeId INTEGER NOT NULL REFERENCES Employees(Id) ON DELETE CASCADE,
    ProjectId  INTEGER NOT NULL REFERENCES Projects(Id)  ON DELETE CASCADE,
    DaysWorked INTEGER NOT NULL CHECK (DaysWorked > 0),
    PRIMARY KEY (EmployeeId, ProjectId)
);
