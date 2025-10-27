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
}