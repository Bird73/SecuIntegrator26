using SecuIntegrator26.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

using SecuIntegrator26.Core.Entities;

namespace SecuIntegrator26.Core.Interfaces
{
    public interface ISchedulerManagementService
    {
        Task<List<JobStatusDto>> GetAllJobsAsync();
        Task TriggerJobAsync(string jobName, string groupName);
        Task PauseJobAsync(string jobName, string groupName);
        Task ResumeJobAsync(string jobName, string groupName);
        
        Task<JobScheduleConfig> GetConfigAsync();
        Task UpdateConfigAsync(JobScheduleConfig config);
        Task RestoreConfigAsync();
    }
}
