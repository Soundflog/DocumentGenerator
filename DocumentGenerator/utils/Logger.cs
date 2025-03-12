using DocumentGenerator.Models;
using Npgsql;

namespace DocumentGenerator.utils;

// Класс для логирования операций в базе данных PostgreSQL
public static class Logger
{
    // Задайте корректные параметры подключения к вашей базе PostgreSQL
    private const string ConnectionString = "Host=localhost;Port=5432;Database=templatefillerapp;Username=postgres;Password=6519";

    static Logger()
    {
        // Создание таблицы, если её нет
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        var sql = @"
                    CREATE TABLE IF NOT EXISTS OperationLogs (
                        Id SERIAL PRIMARY KEY, 
                        Timestamp TIMESTAMP, 
                        OperationType TEXT, 
                        Details TEXT
                    )";
        using var command = new NpgsqlCommand(sql, connection);
        command.ExecuteNonQuery();
    }

    public static void Log(string operationType, string details)
    {
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        var sql = "INSERT INTO OperationLogs (Timestamp, OperationType, Details) VALUES (@timestamp, @operationType, @details)";
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.Now);
        command.Parameters.AddWithValue("@operationType", operationType);
        command.Parameters.AddWithValue("@details", details);
        command.ExecuteNonQuery();
    }
    
    public static List<LogEntry> GetLogs()
    {
        var logs = new List<LogEntry>();
        using var connection = new NpgsqlConnection(ConnectionString);
        connection.Open();
        var sql = "SELECT Timestamp, OperationType, Details FROM OperationLogs ORDER BY Timestamp";
        using var command = new NpgsqlCommand(sql, connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            logs.Add(new LogEntry
            {
                Timestamp = reader.GetDateTime(0),
                OperationType = reader.GetString(1),
                Details = reader.GetString(2)
            });
        }

        return logs;
    }
}