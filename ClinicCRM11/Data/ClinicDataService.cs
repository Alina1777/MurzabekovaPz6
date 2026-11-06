
using ClinicCRM.Models;
using Microsoft.Data.Sqlite;

namespace ClinicCRM.Data;

public class ClinicDataService
{
    private readonly string _connectionString;

    public ClinicDataService(string connectionString)
    {
        _connectionString = connectionString;
    }

    public List<Department> GetDepartments()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Departments";

        using var reader = command.ExecuteReader();

        var result = new List<Department>();

        while (reader.Read())
        {
            result.Add(new Department
            {
                Id = reader.GetInt32(0),
                Title = reader.GetString(1),
                BaseRate = Convert.ToDecimal(reader.GetDouble(2))
            });
        }

        return result;
    }

    public List<Doctor> GetDoctors()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Doctors";

        using var reader = command.ExecuteReader();

        var result = new List<Doctor>();

        while (reader.Read())
        {
            int id = reader.GetInt32(0);
            string name = reader.GetString(1);
            int departmentId = reader.IsDBNull(2)
                ? 0
                : reader.GetInt32(2);

            string category = reader.GetString(3);
            decimal salary =
                Convert.ToDecimal(reader.GetDouble(4));

            if (departmentId > 0 &&
                GetDepartmentTitle(departmentId) == "Хирургия")
            {
                result.Add(new Surgeon(
                    id, name, salary, category,
                    departmentId, 5));
            }
            else
            {
                result.Add(new Doctor(
                    id, name, salary, category, departmentId));
            }
        }

        return result;
    }

    private string GetDepartmentTitle(int id)
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            "SELECT Title FROM Departments WHERE Id=@id";

        command.Parameters.AddWithValue("@id", id);

        return command.ExecuteScalar()?.ToString() ?? "";
    }

    public List<Appointment> GetAppointments()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Appointments";

        using var reader = command.ExecuteReader();

        var result = new List<Appointment>();

        while (reader.Read())
        {
            result.Add(new Appointment
            {
                Id = reader.GetInt32(0),
                DoctorId = reader.IsDBNull(1)
                    ? 0
                    : reader.GetInt32(1),
                PatientName = reader.GetString(2),
                Cost = Convert.ToDecimal(reader.GetDouble(3)),
                Date = DateTime.Parse(reader.GetString(4)),
                Status = reader.GetString(5)
            });
        }

        return result;
    }

    public void AddDoctor(Doctor doctor)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Doctors
            (FullName, DepartmentId, Category, BaseSalary)
            VALUES (@name, @department, @category, @salary)";

        command.Parameters.AddWithValue("@name", doctor.FullName);
        command.Parameters.AddWithValue(
            "@department", doctor.DepartmentId);
        command.Parameters.AddWithValue(
            "@category", doctor.Category);
        command.Parameters.AddWithValue(
            "@salary", doctor.BaseSalary);

        command.ExecuteNonQuery();
    }

    public void AddAppointment(Appointment appointment)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            INSERT INTO Appointments
            (DoctorId, PatientName, Cost, Date, Status)
            VALUES (@doctor, @patient, @cost, @date, @status)";

        command.Parameters.AddWithValue(
            "@doctor", appointment.DoctorId);
        command.Parameters.AddWithValue(
            "@patient", appointment.PatientName);
        command.Parameters.AddWithValue(
            "@cost", appointment.Cost);
        command.Parameters.AddWithValue(
            "@date",
            appointment.Date.ToString("yyyy-MM-dd HH:mm:ss"));
        command.Parameters.AddWithValue(
            "@status", appointment.Status);

        command.ExecuteNonQuery();
    }

    public void UpdateAppointmentStatus(int id, string status)
    {
        if (!new[] { "Planned", "Completed", "Cancelled" }
            .Contains(status))
            throw new ArgumentException("Недопустимый статус.");

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = @"
            UPDATE Appointments
            SET Status = @status
            WHERE Id = @id";

        command.Parameters.AddWithValue("@status", status);
        command.Parameters.AddWithValue("@id", id);

        command.ExecuteNonQuery();
    }

    public List<AppointmentViewDto> GetAppointmentReport()
    {
        var appointments = GetAppointments();
        var doctors = GetDoctors();
        var departments = GetDepartments();

        return (
            from a in appointments
            join d in doctors on a.DoctorId equals d.Id
            join dep in departments
                on d.DepartmentId equals dep.Id
            select new AppointmentViewDto
            {
                Patient = a.PatientName,
                Doctor = d.FullName,
                Department = dep.Title,
                Cost = a.Cost,
                Date = a.Date,
                Status = a.Status
            }).ToList();
    }

    public object GetRevenueByDoctors()
    {
        var appointments = GetAppointments();
        var doctors = GetDoctors();

        var query =
            from a in appointments
            group a by a.DoctorId into g
            join d in doctors on g.Key equals d.Id
            select new
            {
                Doctor = d.FullName,
                AppointmentCount = g.Count(),
                Revenue = g.Sum(x => x.Cost),
                AverageCheck = g.Average(x => x.Cost)
            };

        return query.ToList();
    }

    public List<AppointmentViewDto> GetCompletedSorted()
    {
        return GetAppointmentReport()
            .Where(a => a.Status == "Completed")
            .OrderBy(a => a.Department)
            .ThenByDescending(a => a.Cost)
            .ToList();
    }

    public List<string> GetIntersectPatients()
    {
        var report = GetAppointmentReport();

        var therapy = report
            .Where(a => a.Department == "Терапия")
            .Select(a => a.Patient)
            .Distinct();

        var surgery = report
            .Where(a => a.Department == "Хирургия")
            .Select(a => a.Patient)
            .Distinct();

        return therapy.Intersect(surgery).ToList();
    }

    public List<string> GetExceptPatients()
    {
        var report = GetAppointmentReport();

        var therapy = report
            .Where(a => a.Department == "Терапия")
            .Select(a => a.Patient)
            .Distinct();

        var surgery = report
            .Where(a => a.Department == "Хирургия")
            .Select(a => a.Patient)
            .Distinct();

        return therapy.Except(surgery).ToList();
    }

    public List<AppointmentViewDto> GetPage(
        int page, int pageSize)
    {
        if (page < 1 || pageSize < 1)
            throw new ArgumentOutOfRangeException();

        return GetAppointmentReport()
            .OrderByDescending(a => a.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
