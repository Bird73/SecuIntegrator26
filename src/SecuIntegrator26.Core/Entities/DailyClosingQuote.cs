using System.ComponentModel.DataAnnotations.Schema;

namespace SecuIntegrator26.Core.Entities
{
    public class DailyClosingQuote
    {
        public string StockCode { get; set; } = string.Empty;
        public DateTime TradeDate { get; set; }

        public long TradeVolume { get; set; } // 成交股數
        public long TradeValue { get; set; }  // 成交金額
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal OpeningPrice { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal HighestPrice { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal LowestPrice { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal ClosingPrice { get; set; }
        
        [Column(TypeName = "decimal(18,4)")]
        public decimal Change { get; set; }
        
        public int TransactionCount { get; set; } // 成交筆數

        // Navigation property
        public virtual StockSymbol? StockSymbol { get; set; }
    }
}
