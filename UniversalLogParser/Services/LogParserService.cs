using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Wordprocessing;
using UniversalLogParser.Models;

namespace UniversalLogParser.Services
{
    public class LogParserService : ILogParserService
    {
        private readonly Regex _creatioRegex = new Regex(
            @"^(?<date>\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3})\s+\[\d+\]\s+(?<level>\w+)\s+(?<rest>.+)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private readonly Regex _genericRegex = new Regex(
            @"^(?<date>\d{4}-\d{2}-\d{2}[\sT]\d{2}:\d{2}:\d{2}(?:[.,]\d{3})?)\s*\[(?<level>\w+)\]\s*(?<message>.*)$",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public IEnumerable<LogEntry> Parse(string filePath, int fileId)
        {
            var entries = new List<LogEntry>();
            string content = File.ReadAllText(filePath);

            Regex selectedRegex = DetectLogFormat(content);

            foreach (Match match in selectedRegex.Matches(content))
            {
                try
                {
                    string dateText = match.Groups["date"].Value;
                    string level = match.Groups["level"].Value;
                    string message = match.Groups["message"].Success
                        ? match.Groups["message"].Value
                        : match.Groups["rest"].Value;

                    message = Regex.Replace(
                        message,
                        @"IIS\s+APPPOOL\\\S+\s+",
                        string.Empty,
                        RegexOptions.IgnoreCase
                    );
                    
                    entries.Add(new LogEntry
                    {
                        Date = DateTime.TryParse(dateText, out var dt) ? dt : DateTime.MinValue,
                        Level = DetectMsgLevel(level),
                        Message = message.Trim(),
                        LogFileId = fileId
                    });
                }
                catch
                {}
            }

            return entries;
        }

        private Regex DetectLogFormat(string content)
        {
            if (Regex.IsMatch(content, @"\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2},\d{3}\s+\[\d+\]\s+\w+\s"))
                return _creatioRegex;

            return _genericRegex;
        }

        private string DetectMsgLevel(string content)
        {
            string level;
            switch (content.ToUpper())
            {
                case "INFO":
                    level = "INF"; 
                    break;
                case "WARN":
                    level = "WRN";
                    break;
                case "ERROR":
                    level = "ERR";
                    break;
                case "DEBUG":
                    level = "DBG";
                    break;
                default:
                    level = content;
                    break;
            }
            return level;
              
        }
    }
}