
using Microsoft.Data.Sqlite;

namespace ClinicCRM.Data;

public static class DatabaseInitializer
{
    public static readonly string DatabasePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "clinic.db");

    public static readonly string ConnectionString =
        $"Data Source={DatabasePath}";

    public static void Initialize()
    {
        using var connection = new SqliteConnection(ConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            PRAGMA foreign_keys = ON;

            CREATE TABLE IF NOT EXISTS Departments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                BaseRate REAL NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Doctors (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                DepartmentId INTEGER,
                Category TEXT NOT NULL,
                BaseSalary REAL NOT NULL,
                FOREIGN KEY (DepartmentId)
                    REFERENCES Departments(Id)
            );

            CREATE TABLE IF NOT EXISTS Appointments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                DoctorId INTEGER,
                PatientName TEXT NOT NULL,
                Cost REAL NOT NULL,
                Date TEXT NOT NULL,
                Status TEXT NOT NULL,
                FOREIGN KEY (DoctorId)
                    REFERENCES Doctors(Id)
            );

            CREATE INDEX IF NOT EXISTS IX_Appointments_DoctorId
            ON Appointments(DoctorId);
        ";

        command.ExecuteNonQuery();

        Seed(connection);
    }

    private static void Seed(SqliteConnection connection)
    {
        using var transaction = connection.BeginTransaction();

        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        command.CommandText = @"
            INSERT INTO Departments (Title, BaseRate)
            SELECT 'Терапия', 1000
            WHERE NOT EXISTS (
                SELECT 1 FROM Departments WHERE Title = 'Терапия'
            );

            INSERT INTO Departments (Title, BaseRate)
            SELECT 'Хирургия', 1500
            WHERE NOT EXISTS (
                SELECT 1 FROM Departments WHERE Title = 'Хирургия'
            );

            INSERT INTO Departments (Title, BaseRate)
            SELECT 'Кардиология', 1300
            WHERE NOT EXISTS (
                SELECT 1 FROM Departments WHERE Title = 'Кардиология'
            );

            INSERT INTO Doctors
                (FullName, DepartmentId, Category, BaseSalary)
            SELECT
                'Иванов Иван Иванович',
                (SELECT Id FROM Departments WHERE Title='Терапия'),
                'Высшая', 60000
            WHERE NOT EXISTS (
                SELECT 1 FROM Doctors
                WHERE FullName='Иванов Иван Иванович'
            );

            INSERT INTO Doctors
                (FullName, DepartmentId, Category, BaseSalary)
            SELECT
                'Петров Пётр Сергеевич',
                (SELECT Id FROM Departments WHERE Title='Хирургия'),
                'Первая', 85000
            WHERE NOT EXISTS (
                SELECT 1 FROM Doctors
                WHERE FullName='Петров Пётр Сергеевич'
            );

            INSERT INTO Doctors
                (FullName, DepartmentId, Category, BaseSalary)
            SELECT
                'Сидорова Анна Викторовна',
                (SELECT Id FROM Departments WHERE Title='Кардиология'),
                'Высшая', 75000
            WHERE NOT EXISTS (
                SELECT 1 FROM Doctors
                WHERE FullName='Сидорова Анна Викторовна'
            );

            INSERT INTO Appointments
                (DoctorId, PatientName, Cost, Date, Status)
            SELECT
                d.Id, 'Алексей Смирнов', 2500,
                '2026-11-01 10:00:00', 'Completed'
            FROM Doctors d
            WHERE d.FullName='Иванов Иван Иванович'
              AND NOT EXISTS (
                SELECT 1 FROM Appointments
                WHERE PatientName='Алексей Смирнов'
                  AND DoctorId=d.Id
            );

            INSERT INTO Appointments
                (DoctorId, PatientName, Cost, Date, Status)
            SELECT
                d.Id, 'Мария Кузнецова', 4500,
                '2026-11-02 11:00:00', 'Completed'
            FROM Doctors d
            WHERE d.FullName='Петров Пётр Сергеевич'
              AND NOT EXISTS (
                SELECT 1 FROM Appointments
                WHERE PatientName='Мария Кузнецова'
                  AND DoctorId=d.Id
            );

            INSERT INTO Appointments
                (DoctorId, PatientName, Cost, Date, Status)
            SELECT
                d.Id, 'Алексей Смирнов', 3000,
                '2026-11-03 12:00:00', 'Completed'
            FROM Doctors d
            WHERE d.FullName='Петров Пётр Сергеевич'
              AND NOT EXISTS (
                SELECT 1 FROM Appointments
                WHERE PatientName='Алексей Смирнов'
                  AND DoctorId=d.Id
            );

            INSERT INTO Appointments
                (DoctorId, PatientName, Cost, Date, Status)
            SELECT
                d.Id, 'Елена Волкова', 3500,
                '2026-11-04 13:00:00', 'Planned'
            FROM Doctors d
            WHERE d.FullName='Сидорова Анна Викторовна'
              AND NOT EXISTS (
                SELECT 1 FROM Appointments
                WHERE PatientName='Елена Волкова'
                  AND DoctorId=d.Id
            );
        ";

        command.ExecuteNonQuery();
        transaction.Commit();
    }
}
