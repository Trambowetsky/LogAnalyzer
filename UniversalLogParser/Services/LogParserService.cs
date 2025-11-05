using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UniversalLogParser.Models;

namespace UniversalLogParser.Services
{
    public class LogParserService : ILogParserService
    {
        private readonly Regex _regexWithLevel = new Regex(
            @"^(?<date>\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2}(?:[.,]\d{3})?)\s*\[(?<level>\w+)\]\s*(?<message>.*)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private readonly Regex _regexWithoutLevel = new Regex(
            @"^(?<date>\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2}(?:[.,]\d{3})?)\s+(?<message>.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public IEnumerable<LogEntry> Parse(string filePath, int fileId)
        {
            var content = File.ReadAllText(filePath);
            var entries = new List<LogEntry>();
            
            bool hasLevels = content.Contains("[ERR]") || content.Contains("[WRN]") ||
                             content.Contains("[INF]") || content.Contains("[DBG]");

            var regex = hasLevels ? _regexWithLevel : _regexWithoutLevel;

            foreach (Match match in regex.Matches(content))
            {
                string level = hasLevels ? match.Groups["level"].Value : "UNK";

                entries.Add(new LogEntry
                {
                    Date = DateTime.TryParse(match.Groups["date"].Value, out var date) ? date : DateTime.MinValue,
                    Level = level,
                    Message = match.Groups["message"].Value.Trim(),
                    LogFileId = fileId
                });
            }

            return entries;
        }
    }
}