using System.Globalization;

namespace SecuIntegrator26.Shared.Utils
{
    public static class DateHelper
    {
        /// <summary>
        /// 將民國年字串 (e.g., "112/01/01" 或 "1120101") 轉換為 DateTime
        /// </summary>
        public static DateTime? ParseTwseDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString)) return null;

            dateString = dateString.Replace("/", "").Replace("-", "");
            
            if (dateString.Length >= 7 && int.TryParse(dateString.Substring(0, 3), out int year))
            {
                int adYear = year + 1911;
                string adDateString = adYear + dateString.Substring(3);
                
                if (DateTime.TryParseExact(adDateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }
            return null;
        }

        public static string ToTwseDateString(this DateTime date)
        {
            int twYear = date.Year - 1911;
            return $"{twYear}/{date:MM/dd}";
        }
    }
}
