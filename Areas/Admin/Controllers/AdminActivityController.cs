using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebActionResults.Services;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminActivityController : AdminControllerBase
{
    private readonly ITraceLogService _traceService;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true
    };

    public AdminActivityController(ITraceLogService traceService)
    {
        _traceService = traceService;
    }

    // ── Main Dashboard ────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Trung tâm Hoạt động & Bảo mật";
        var summary = await _traceService.GetSummaryAsync();
        return View(summary);
    }

    // ── AJAX: Events list ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Events(
        string? search,
        string? actionType,
        string? ip,
        string? country,
        string? riskLevel,
        string? actorType,
        string? dateFrom,
        string? dateTo,
        int page = 1,
        int pageSize = 20)
    {
        var filter = new TraceFilter
        {
            Search = search,
            ActionType = actionType,
            Ip = ip,
            Country = country,
            RiskLevel = riskLevel,
            ActorType = actorType,
            DateFrom = string.IsNullOrEmpty(dateFrom) ? null : DateTimeOffset.TryParse(dateFrom, out var df) ? df : null,
            DateTo = string.IsNullOrEmpty(dateTo) ? null : DateTimeOffset.TryParse(dateTo, out var dt) ? dt : null,
            Page = page,
            PageSize = pageSize
        };

        var result = await _traceService.GetEventsAsync(filter);
        return Json(result, _jsonOpts);
    }

    // ── AJAX: Single event detail ─────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Detail(string id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var evt = await _traceService.GetEventByIdAsync(id);
        if (evt == null)
            return NotFound();

        return Json(evt, _jsonOpts);
    }

    // ── AJAX: Summary refresh ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Summary()
    {
        var summary = await _traceService.GetSummaryAsync();
        return Json(summary, _jsonOpts);
    }

    // ── Export ────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Export()
    {
        var json = await _traceService.ExportJsonAsync();
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        var fileName = $"trace-logs-{DateTime.Now:yyyyMMdd-HHmmss}.json";
        return File(bytes, "application/json", fileName);
    }

    // ── Clear Logs ────────────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> Clear()
    {
        await _traceService.ClearLogsAsync();
        TempData["Success"] = "Đã xóa toàn bộ nhật ký hoạt động.";
        return RedirectToAction(nameof(Index));
    }
}
