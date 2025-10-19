using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UniversalLogParser.Models;
namespace UniversalLogParser.Services;

public class LogParserService : ILogParserService
{
    private readonly Regex _regex = new Regex(
        @"^(?<date>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}) \[(?<level>\w+)\] (?<message>(?:.|\r?\n(?!\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3}))+)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    public IEnumerable<LogEntry> Parse(string filePath, int fileId)
    {
        var entries = new List<LogEntry>();

        foreach (var line in File.ReadAllLines(filePath))
        {
            var match = _regex.Match(line);
            if (match.Success)
            {
                entries.Add(new LogEntry
                {
                    Date = DateTime.Parse(match.Groups["date"].Value),
                    Level = match.Groups["level"].Value,
                    Message = match.Groups["message"].Value,
                    LogFileId = fileId
                });
            }
        }

        return entries;
    }
}