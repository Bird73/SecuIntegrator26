using System.ComponentModel.DataAnnotations.Schema;

namespace SecuIntegrator26.Core.Entities
{
    public class MonthlyRevenue
    {
        public string StockCode { get; set; } = string.Empty;
        public string YearMonth { get; set; } = string.Empty; // YYYYMM

        [Column(TypeName = "decimal(18,2)")]
        public decimal RevenueCurrent { get; set; } // 當月營收

        [Column(TypeName = "decimal(18,2)")]
        public decimal RevenueLastYear { get; set; } // 去年同月營收

        [Column(TypeName = "decimal(18,2)")]
        public decimal MomChange { get; set; } // 上月比較增減(%)

        [Column(TypeName = "decimal(18,2)")]
        public decimal YoyChange { get; set; } // 去年同月增減(%)

        // Navigation property
        public virtual StockSymbol? StockSymbol { get; set; }
    }
}
