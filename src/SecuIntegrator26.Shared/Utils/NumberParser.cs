using System.Globalization;

namespace SecuIntegrator26.Shared.Utils
{
    public static class NumberParser
    {
        public static decimal ParseDecimal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0m;
            // 移除千分位逗號，處理括號代表負數的情況
            value = value.Replace(",", "");
            
            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return 0m;
        }

        public static long ParseLong(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return 0L;
             value = value.Replace(",", "");
            
            if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out long result))
            {
                return result;
            }
            return 0L;
        }
    }
}
