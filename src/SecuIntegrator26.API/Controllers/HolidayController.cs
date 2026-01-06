using Microsoft.AspNetCore.Mvc;
using SecuIntegrator26.Core.Entities;
using SecuIntegrator26.Core.Interfaces;
using System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecuIntegrator26.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HolidayController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidayController(IHolidayService holidayService)
        {
            _holidayService = holidayService;
        }

        [HttpGet("{year}")]
        public async Task<ActionResult<IReadOnlyList<HolidayConfig>>> GetHolidays(int year)
        {
            var holidays = await _holidayService.GetHolidaysAsync(year);
            return Ok(holidays);
        }

        [HttpPost]
        public async Task<ActionResult> AddHoliday([FromBody] HolidayConfig request)
        {
            await _holidayService.AddHolidayAsync(request.Date, request.Description);
            return Ok();
        }

        [HttpDelete("{date}")]
        public async Task<ActionResult> RemoveHoliday(DateTime date)
        {
            await _holidayService.RemoveHolidayAsync(date);
            return Ok();
        }

        [HttpPost("import")]
        public async Task<ActionResult> ImportHolidays([FromBody] ImportRequest request)
        {
            // For simplicity, we assume the file path is passed or handles file upload differently.
            // Here we might just accept JSON content directly in a real app, but the service expects a path.
            // For this MVP, let's assume we pass a path or implement a temporary file mechanism if needed.
            // BUT, looking at IHolidayService.ImportHolidaysFromJsonAsync, it takes a filePath.
            // Let's stick to the service definition for now, or adapt if needed.
            // User requirement mentioned JSON I/O via file.
            
            try 
            {
                 if (string.IsNullOrEmpty(request.JsonFilePath)) return BadRequest("File path is required");
                 await _holidayService.ImportHolidaysFromJsonAsync(request.JsonFilePath);
                 return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpPost("export/{year}")]
        public async Task<ActionResult> ExportHolidays(int year, [FromBody] ExportRequest request)
        {
             if (string.IsNullOrEmpty(request.JsonFilePath)) return BadRequest("File path is required");
             await _holidayService.ExportHolidaysToJsonAsync(year, request.JsonFilePath);
             return Ok();
        }

        [HttpPost("upload")]
        public async Task<ActionResult> UploadHolidays(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File not selected");

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var content = await reader.ReadToEndAsync();
                await _holidayService.ImportHolidaysFromJsonContentAsync(content);
            }
            return Ok();
        }

        [HttpGet("download/{year}")]
        public async Task<IActionResult> DownloadHolidays(int year)
        {
            var holidays = await _holidayService.GetHolidaysAsync(year);
            var json = JsonSerializer.Serialize(holidays, new JsonSerializerOptions { WriteIndented = true });
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"holidays_{year}.json");
        }
    }

    public class ImportRequest
    {
        public string JsonFilePath { get; set; } = string.Empty;
    }

    public class ExportRequest
    {
        public string JsonFilePath { get; set; } = string.Empty;
    }
}
