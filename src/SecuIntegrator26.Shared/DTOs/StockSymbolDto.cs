namespace SecuIntegrator26.Shared.DTOs
{
    public class StockSymbolDto
    {
        public string StockCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string MarketType { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
