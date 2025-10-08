using UniversalLogParser.Models;
namespace UniversalLogParser.Services;

public interface ILogParserService
{
    IEnumerable<LogEntry> Parse(string filePath);
}