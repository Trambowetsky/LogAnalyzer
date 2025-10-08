namespace UniversalLogParser.Models;

public class LogEntry
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; 
    public string Message { get; set; } = string.Empty;
}