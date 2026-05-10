using Microsoft.EntityFrameworkCore;
using WebActionResults.Data.Entities;
using WebActionResults.Models;

namespace WebActionResults.Data.Services;

public interface IWebSettingsService
{
    Task<Dictionary<string, string>> GetAllSettingsAsync();
    Task<string?> GetSettingAsync(string key);
    Task<bool> UpdateSettingAsync(string key, string value);
}

public class WebSettingsService : IWebSettingsService
{
    private readonly ShopDbContext _context;

    public WebSettingsService(ShopDbContext context)
    {
        _context = context;
    }

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
    {
        var settings = await _context.WebSettings.ToListAsync();
        return settings.ToDictionary(s => s.SettingKey, s => s.SettingValue ?? string.Empty);
    }

    public async Task<string?> GetSettingAsync(string key)
    {
        var setting = await _context.WebSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);
        return setting?.SettingValue;
    }

    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
        var setting = await _context.WebSettings
            .FirstOrDefaultAsync(s => s.SettingKey == key);

        if (setting == null)
        {
            setting = new WebSettings
            {
                SettingKey = key,
                SettingValue = value,
                CreatedAt = DateTime.UtcNow
            };
            _context.WebSettings.Add(setting);
        }
        else
        {
            setting.SettingValue = value;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}