using SecuIntegrator26.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface IHolidayService
    {
        Task<IReadOnlyList<HolidayConfig>> GetHolidaysAsync(int year);
        Task AddHolidayAsync(DateTime date, string description);
        Task RemoveHolidayAsync(DateTime date);
        Task<bool> IsHolidayAsync(DateTime date);
        
        /// <summary>
        /// 從 JSON 檔案匯入休假日設定
        /// </summary>
        Task ImportHolidaysFromJsonAsync(string jsonFilePath);
        Task ImportHolidaysFromJsonContentAsync(string jsonContent);
        
        /// <summary>
        /// 匯出該年度休假日設定為 JSON
        /// </summary>
        Task ExportHolidaysToJsonAsync(int year, string jsonFilePath);
    }
}
