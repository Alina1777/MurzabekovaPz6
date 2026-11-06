
using ClinicCRM.Interfaces;
using Microsoft.Data.Sqlite;

namespace ClinicCRM.Data;

public class SqliteRepository<T> : IRepository<T> where T : class
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly Func<SqliteDataReader, T> _mapper;
    private readonly Action<SqliteCommand, T> _insertMapper;

    private static readonly HashSet<string> AllowedTables =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Doctors",
            "Departments",
            "Appointments"
        };

    public SqliteRepository(
        string connectionString,
        string tableName,
        Func<SqliteDataReader, T> mapper,
        Action<SqliteCommand, T> insertMapper)
    {
        if (!AllowedTables.Contains(tableName))
            throw new ArgumentException("Недопустимое имя таблицы.");

        _connectionString = connectionString;
        _tableName = tableName;
        _mapper = mapper;
        _insertMapper = insertMapper;
    }

    public List<T> GetAll()
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM [{_tableName}]";

        using var reader = command.ExecuteReader();

        var result = new List<T>();

        while (reader.Read())
            result.Add(_mapper(reader));

        return result;
    }

    public T GetById(int id)
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            $"SELECT * FROM [{_tableName}] WHERE Id = @id";

        command.Parameters.AddWithValue("@id", id);

        using var reader = command.ExecuteReader();

        if (reader.Read())
            return _mapper(reader);

        throw new KeyNotFoundException(
            $"{_tableName}: запись с Id={id} не найдена.");
    }

    public void Add(T item)
    {
        using var connection =
            new SqliteConnection(_connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        _insertMapper(command, item);
        command.ExecuteNonQuery();
    }
}
