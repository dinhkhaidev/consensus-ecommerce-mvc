using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminReviewController : AdminControllerBase
{
    private readonly ShopDbContext _context;

    public AdminReviewController(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? filter, int page = 1, int pageSize = 15)
    {
        var query = _context.Reviews
            .Include(r => r.Product)
            .Include(r => r.User)
            .AsQueryable();

        if (filter == "approved")
        {
            query = query.Where(r => r.IsApproved);
        }
        else if (filter == "pending")
        {
            query = query.Where(r => !r.IsApproved);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var reviews = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["Filter"] = filter;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(reviews);
    }

    [HttpPost]
    public async Task<IActionResult> Approve(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            review.IsApproved = true;
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Review approved successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Reject(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            review.IsApproved = false;
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Review rejected!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var review = await _context.Reviews.FindAsync(id);
        if (review != null)
        {
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Review deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> BulkAction(string action, List<int> reviewIds)
    {
        if (reviewIds == null || !reviewIds.Any())
        {
            TempData["ToastError"] = "No reviews selected.";
            return RedirectToAction(nameof(Index));
        }

        var reviews = await _context.Reviews.Where(r => reviewIds.Contains(r.Id)).ToListAsync();

        foreach (var review in reviews)
        {
            switch (action)
            {
                case "approve":
                    review.IsApproved = true;
                    break;
                case "reject":
                    review.IsApproved = false;
                    break;
                case "delete":
                    _context.Reviews.Remove(review);
                    break;
            }
        }

        await _context.SaveChangesAsync();
        TempData["ToastSuccess"] = $"Bulk action '{action}' completed for {reviewIds.Count} review(s).";
        return RedirectToAction(nameof(Index));
    }
}
