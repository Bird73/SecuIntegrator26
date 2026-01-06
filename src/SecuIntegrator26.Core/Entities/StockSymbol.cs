using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecuIntegrator26.Core.Entities
{
    public class StockSymbol
    {
        [Key]
        [MaxLength(20)]
        public string StockCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string MarketType { get; set; } = string.Empty; // Listed, OTC, Emerging

        [MaxLength(50)]
        public string Industry { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
