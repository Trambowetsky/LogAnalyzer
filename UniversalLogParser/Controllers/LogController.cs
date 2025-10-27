using Microsoft.AspNetCore.Mvc;
using UniversalLogParser.Data;
using UniversalLogParser.Models;
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

    private static bool ContainsAny(string? text, params string[] keywords)
    {
        if (string.IsNullOrEmpty(text)) return false;
        return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}