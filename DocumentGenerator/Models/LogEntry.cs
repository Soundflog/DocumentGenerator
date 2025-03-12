namespace DocumentGenerator.Models;

public class LogEntry
{
    public System.DateTime Timestamp { get; set; }
    public string OperationType { get; set; }
    public string Details { get; set; }
}