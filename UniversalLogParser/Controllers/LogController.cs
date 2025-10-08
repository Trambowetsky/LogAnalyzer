using Microsoft.AspNetCore.Mvc;
using UniversalLogParser.Data;
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
    public IActionResult Index()
    {
        var logs = _context.LogEntries.OrderByDescending(x => x.Date).Take(100).ToList();
        return View(logs);
    }
    [HttpPost]
    public IActionResult Upload(IFormFile file)
    {
        if (file != null && file.Length > 0)
        {
            var tempPath = Path.GetTempFileName();
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            var entries = _parser.Parse(tempPath);
            _context.LogEntries.AddRange(entries);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }
}