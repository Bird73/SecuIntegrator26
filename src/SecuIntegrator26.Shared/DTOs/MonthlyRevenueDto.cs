namespace SecuIntegrator26.Shared.DTOs
{
    public class MonthlyRevenueDto
    {
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public string YearMonth { get; set; } = string.Empty; // YYYYMM
        public decimal RevenueCurrent { get; set; }
        public decimal RevenueLastYear { get; set; }
        public decimal MomChange { get; set; }
        public decimal YoyChange { get; set; }
    }
}
