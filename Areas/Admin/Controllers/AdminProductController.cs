using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Areas.Admin.Controllers;

[Area("Admin")]
public class AdminProductController : AdminControllerBase
{
    private readonly ShopDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public AdminProductController(ShopDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.ProductName!.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var products = await query
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .OrderBy(p => p.ProductName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["SearchTerm"] = search;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;
        ViewData["TotalCount"] = totalCount;
        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        return View(product);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product, List<IFormFile>? imageFiles, List<ProductVariantInput>? variants)
    {
        if (ModelState.IsValid)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            await SaveProductVariantsAsync(product.Id, variants);
            await SaveProductImagesAsync(product.Id, product.ProductName, imageFiles, hasMainImage: false);
            TempData["ToastSuccess"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Product product, List<IFormFile>? imageFiles, List<ProductVariantInput>? variants)
    {
        if (ModelState.IsValid)
        {
            var existingProduct = await _context.Products.FindAsync(product.Id);
            if (existingProduct == null) return NotFound();

            existingProduct.ProductName = product.ProductName;
            existingProduct.QuantityPerUnit = product.QuantityPerUnit;
            existingProduct.UnitPrice = product.UnitPrice;
            existingProduct.CategoryID = product.CategoryID;
            existingProduct.SupplierID = product.SupplierID;
            existingProduct.Discontinued = product.Discontinued;

            await _context.SaveChangesAsync();
            await SaveProductVariantsAsync(product.Id, variants);
            var hasMainImage = await _context.ProductImages.AnyAsync(i => i.ProductId == product.Id && i.IsMain);
            await SaveProductImagesAsync(product.Id, product.ProductName, imageFiles, hasMainImage);
            TempData["ToastSuccess"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.Categories = _context.Categories.OrderBy(c => c.CategoryName).ToList();
        ViewBag.Suppliers = _context.Suppliers.OrderBy(s => s.CompanyName).ToList();
        return View(product);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            TempData["ToastSuccess"] = "Product deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task SaveProductVariantsAsync(int productId, List<ProductVariantInput>? variants)
    {
        if (variants == null || variants.Count == 0)
            return;

        foreach (var input in variants)
        {
            var hasValue = !string.IsNullOrWhiteSpace(input.Color)
                || !string.IsNullOrWhiteSpace(input.Size)
                || !string.IsNullOrWhiteSpace(input.SKU)
                || input.PriceAdjustment.HasValue
                || input.StockQuantity > 0;

            if (!hasValue && input.Id <= 0)
                continue;

            if (input.Id > 0)
            {
                var existing = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == input.Id && v.ProductId == productId);
                if (existing == null)
                    continue;

                if (input.Remove)
                {
                    existing.IsActive = false;
                    existing.StockQuantity = 0;
                    continue;
                }

                existing.Color = input.Color?.Trim();
                existing.Size = input.Size?.Trim();
                existing.SKU = input.SKU?.Trim();
                existing.PriceAdjustment = input.PriceAdjustment ?? 0;
                existing.StockQuantity = Math.Max(0, input.StockQuantity);
                existing.IsActive = input.IsActive;
                continue;
            }

            if (input.Remove)
                continue;

            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Color = input.Color?.Trim(),
                Size = input.Size?.Trim(),
                SKU = input.SKU?.Trim(),
                PriceAdjustment = input.PriceAdjustment ?? 0,
                StockQuantity = Math.Max(0, input.StockQuantity),
                IsActive = input.IsActive
            });
        }

        await _context.SaveChangesAsync();
    }

    private async Task SaveProductImagesAsync(int productId, string? productName, List<IFormFile>? imageFiles, bool hasMainImage)
    {
        if (imageFiles == null || imageFiles.Count == 0)
            return;

        var validFiles = imageFiles.Where(f => f is { Length: > 0 }).ToList();
        if (!validFiles.Any())
            return;

        var maxDisplayOrder = await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .Select(i => (int?)i.DisplayOrder)
            .MaxAsync() ?? 0;

        var images = new List<ProductImage>();
        foreach (var file in validFiles)
        {
            var imageUrl = await SaveProductImageFileAsync(file);
            if (string.IsNullOrEmpty(imageUrl))
                continue;

            maxDisplayOrder++;
            images.Add(new ProductImage
            {
                ProductId = productId,
                ImageUrl = imageUrl,
                AltText = productName,
                DisplayOrder = maxDisplayOrder,
                IsMain = !hasMainImage && images.Count == 0
            });
        }

        if (images.Any())
        {
            _context.ProductImages.AddRange(images);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<string?> SaveProductImageFileAsync(IFormFile file)
    {
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        var extension = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(extension) || !allowedExtensions.Contains(extension))
            return null;

        if (file.Length > 5 * 1024 * 1024)
            return null;

        var folder = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var path = Path.Combine(folder, fileName);

        await using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);

        return $"/uploads/products/{fileName}";
    }
}

public class ProductVariantInput
{
    public int Id { get; set; }
    public string? Color { get; set; }
    public string? Size { get; set; }
    public string? SKU { get; set; }
    public decimal? PriceAdjustment { get; set; }
    public int StockQuantity { get; set; }
    public bool IsActive { get; set; } = true;
    public bool Remove { get; set; }
}
