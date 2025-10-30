using Microsoft.AspNetCore.Mvc;
using UniversalLogParser.Data;
using UniversalLogParser.Models;
using ClosedXML.Excel;
using UniversalLogParser.Services;

namespace UniversalLogParser.Controllers;

public class LogController : Controller
{
    private readonly AppDbContext _context;
    private readonly ILogParserService _parser;
    
    public LogController(AppDbContext context, ILogParserService parser)
    {
        _context = context;
        _parser = parser;
    }
    [HttpGet]
    public IActionResult Index(int id)
    {
        var logs = _context.LogEntries
            .Where(x => x.LogFileId == id)
            .OrderByDescending(x => x.Date)
            .Take(100)
            .ToList();

        ViewBag.FileId = id;
        return View(logs);
    }
    [HttpGet]
    public IActionResult Stats(int id)
    {
        var file = _context.LogFiles.FirstOrDefault(f => f.Id == id);
        if (file == null) return NotFound();

        ViewBag.FileId = id;
        ViewBag.FileName = file.FileName;

        return View();
    }
    [HttpGet]
    public IActionResult GetLevelStats(int fileId)
    {
        var stats = _context.LogEntries
            .Where(e => e.LogFileId == fileId)
            .GroupBy(e => e.Level)
            .Select(g => new
            {
                Level = g.Key,
                Count = g.Count()
            })
            .ToList();

        return Json(stats);
    }
    [HttpGet]
    public IActionResult GetPeriodStats(int fileId)
    {
        var activity = _context.LogEntries
            .Where(e => e.LogFileId == fileId)
            .AsEnumerable()
            .GroupBy(e => e.Date.ToString("HH:mm"))
            .Select(g => new
            {
                Time = g.Key,
                Count = g.Count()
            })
            .OrderBy(t => t.Time)
            .ToList();

        return Json(activity);
    }
    [HttpGet]
    public IActionResult GetCategoryStats(int fileId)
    {
        var entries = _context.LogEntries
            .Where(e => e.LogFileId == fileId)
            .Select(e => e.Message)
            .AsEnumerable();

        var categories = new Dictionary<string, int>
        {
            ["Database"] = entries.Count(m => ContainsAny(m, "database", "sql", "query", "db")),
            ["Network"] = entries.Count(m => ContainsAny(m, "timeout", "connection", "network", "http")),
            ["UI"] = entries.Count(m => ContainsAny(m, "button", "window", "ui", "render")),
            ["File"] = entries.Count(m => ContainsAny(m, "file", "path", "directory", "read", "write")),
            ["Other"] = 0
        };

        int total = entries.Count();
        int categorized = categories.Sum(x => x.Value);
        categories["Other"] = total - categorized;

        return Json(categories);
    }
    [HttpGet]
    public IActionResult GetHeatmapData(int fileId)
    {
        var entries = _context.LogEntries
            .Where(e => e.LogFileId == fileId)
            .ToList();

        if (!entries.Any())
            return Json(Array.Empty<object>());

        
        var minDate = entries.Min(e => e.Date);
        var maxDate = entries.Max(e => e.Date);
        var totalMinutes = (maxDate - minDate).TotalMinutes;

        string format =
            totalMinutes <= 180 ? "HH:mm" : 
            totalMinutes <= 1440 ? "HH" :   
            "MM-dd";                        

        var data = entries
            .GroupBy(e => new { TimeKey = e.Date.ToString(format), e.Level })
            .Select(g => new
            {
                x = g.Key.TimeKey,
                y = g.Key.Level,
                v = g.Count()
            })
            .ToList();

        return Json(data);
    }
    [HttpPost]
    public IActionResult ExportToExcel(int fileId, [FromBody] List<int>? selectedIds)
    {
        var logsQuery = _context.LogEntries.Where(e => e.LogFileId == fileId);

        if (selectedIds != null && selectedIds.Any())
        {
            logsQuery = logsQuery.Where(e => selectedIds.Contains(e.Id));
        }

        var logs = logsQuery
            .OrderByDescending(e => e.Date)
            .Select(e => new
            {
                e.Date,
                e.Level,
                e.Message
            })
            .ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Logs");
        
        ws.Cell(1, 1).Value = "Date";
        ws.Cell(1, 2).Value = "Level";
        ws.Cell(1, 3).Value = "Message";
        
        var headerRange = ws.Range(1, 1, 1, 3);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        for (int i = 0; i < logs.Count; i++)
        {
            var log = logs[i];
            ws.Cell(i + 2, 1).Value = log.Date.ToString("yyyy-MM-dd HH:mm:ss");
            ws.Cell(i + 2, 2).Value = log.Level;
            ws.Cell(i + 2, 3).Value = log.Message;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"log_{fileId}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx");
    }
    private static bool ContainsAny(string? text, params string[] keywords)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}