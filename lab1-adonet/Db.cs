using System.Data;
using Microsoft.Data.Sqlite;

namespace ProjectContracts;

/// <summary>Доступ до БД через ADO.NET (Connection / Command / DataReader / Parameters).</summary>
public static class Db
{
    private static readonly string DbPath = Path.Combine(AppContext.BaseDirectory, "contracts.db");
    private static string ConnStr => new SqliteConnectionStringBuilder { DataSource = DbPath, ForeignKeys = true }.ToString();

    /// <summary>Створює структуру та наповнює БД, якщо файлу ще немає.</summary>
    public static void EnsureCreated()
    {
        if (File.Exists(DbPath)) return;
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var tx = conn.BeginTransaction();
        foreach (var file in new[] { "schema.sql", "data.sql" })
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Sql", file));
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    /// <summary>Виконує SELECT і повертає результат у DataTable.</summary>
    public static DataTable Query(string sql, params (string Name, object Value)[] parameters)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        foreach (var (name, value) in parameters)
            cmd.Parameters.AddWithValue(name, value);
        using var reader = cmd.ExecuteReader();
        var table = new DataTable();
        table.Load(reader);
        return table;
    }

    /// <summary>Повертає одне значення (агрегат).</summary>
    public static object? Scalar(string sql)
    {
        using var conn = new SqliteConnection(ConnStr);
        conn.Open();
        using var cmd = new SqliteCommand(sql, conn);
        return cmd.ExecuteScalar();
    }
}
