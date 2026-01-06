using Microsoft.Extensions.Logging;
using SecuIntegrator26.Core.Entities;
using SecuIntegrator26.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecuIntegrator26.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IRepository<HolidayConfig> _repository;
        private readonly IFileService _fileService;
        private readonly ILogger<HolidayService> _logger;

        public HolidayService(IRepository<HolidayConfig> repository, IFileService fileService, ILogger<HolidayService> logger)
        {
            _repository = repository;
            _fileService = fileService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<HolidayConfig>> GetHolidaysAsync(int year)
        {
            var all = await _repository.ListAllAsync();
            return all.Where(h => h.Date.Year == year).OrderBy(h => h.Date).ToList();
        }

        public async Task AddHolidayAsync(DateTime date, string description)
        {
            var existing = await _repository.GetByIdAsync(date);
            if (existing == null)
            {
                await _repository.AddAsync(new HolidayConfig
                {
                    Date = date.Date,
                    Description = description,
                    IsHoliday = true
                });
            }
            else
            {
                existing.Description = description;
                existing.IsHoliday = true; // Ensure it's marked as holiday
                await _repository.UpdateAsync(existing);
            }
        }

        public async Task RemoveHolidayAsync(DateTime date)
        {
            var existing = await _repository.GetByIdAsync(date);
            if (existing != null)
            {
                await _repository.DeleteAsync(existing);
            }
        }

        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            // Weekend check
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                return true;
            }

            // DB check
            var config = await _repository.GetByIdAsync(date.Date);
            return config != null && config.IsHoliday;
        }

        public async Task ImportHolidaysFromJsonAsync(string jsonFilePath)
        {
            var jsonContent = await _fileService.ReadTextAsync(jsonFilePath);
            if (string.IsNullOrEmpty(jsonContent))
            {
                _logger.LogWarning("Holiday JSON file not found or empty: {Path}", jsonFilePath);
                return;
            }
            await ImportHolidaysFromJsonContentAsync(jsonContent);
        }

        public async Task ImportHolidaysFromJsonContentAsync(string jsonContent)
        {
            try
            {
                var holidays = JsonSerializer.Deserialize<List<HolidayConfig>>(jsonContent);
                if (holidays != null)
                {
                    foreach (var h in holidays)
                    {
                        await AddHolidayAsync(h.Date, h.Description);
                    }
                    _logger.LogInformation("Imported {Count} holidays from content", holidays.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse holiday JSON content");
                throw;
            }
        }

        public async Task ExportHolidaysToJsonAsync(int year, string jsonFilePath)
        {
            var holidays = await GetHolidaysAsync(year);
            var json = JsonSerializer.Serialize(holidays, new JsonSerializerOptions { WriteIndented = true });
            await _fileService.SaveTextAsync(jsonFilePath, json);
            _logger.LogInformation("Exported {Count} holidays for year {Year} to {Path}", holidays.Count, year, jsonFilePath);
        }
    }
}
