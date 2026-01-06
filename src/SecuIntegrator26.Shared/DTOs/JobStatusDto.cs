using System;

namespace SecuIntegrator26.Shared.DTOs
{
    public class JobStatusDto
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? NextFireTime { get; set; }
        public DateTime? PreviousFireTime { get; set; }
        public string TriggerState { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
    }
}
