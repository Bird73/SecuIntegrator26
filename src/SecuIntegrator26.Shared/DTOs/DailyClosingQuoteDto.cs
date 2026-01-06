namespace SecuIntegrator26.Shared.DTOs
{
    public class DailyClosingQuoteDto
    {
        public string StockCode { get; set; } = string.Empty;
        public string StockName { get; set; } = string.Empty;
        public DateTime TradeDate { get; set; }
        public long TradeVolume { get; set; }
        public long TradeValue { get; set; }
        public decimal OpeningPrice { get; set; }
        public decimal HighestPrice { get; set; }
        public decimal LowestPrice { get; set; }
        public decimal ClosingPrice { get; set; }
        public decimal Change { get; set; }
        public int TransactionCount { get; set; }
    }
}
