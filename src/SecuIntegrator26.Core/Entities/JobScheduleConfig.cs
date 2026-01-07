using System.Collections.Generic;

namespace SecuIntegrator26.Core.Entities
{
    public class JobScheduleConfig
    {
        public DateTime DataStartDate { get; set; } = new DateTime(2025, 1, 1);
        public List<JobSetting> Jobs { get; set; } = new List<JobSetting>();
    }

    public class JobSetting
    {
        public string JobName { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty; // Backward compatibility
        public List<string> CronExpressions { get; set; } = new List<string>();
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = string.Empty;
    }
}
