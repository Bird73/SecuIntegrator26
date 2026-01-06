namespace SecuIntegrator26.Core.Entities
{
    public class HolidayConfig
    {
        public DateTime Date { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsHoliday { get; set; }
    }
}
