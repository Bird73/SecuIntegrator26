using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using SecuIntegrator26.Core.Interfaces;
using SecuIntegrator26.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecuIntegrator26.Services
{
    public class SchedulerManagementService : ISchedulerManagementService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ILogger<SchedulerManagementService> _logger;

        public SchedulerManagementService(ISchedulerFactory schedulerFactory, ILogger<SchedulerManagementService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _logger = logger;
        }

        public async Task<List<JobStatusDto>> GetAllJobsAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            var result = new List<JobStatusDto>();
            var jobGroups = await scheduler.GetJobGroupNames();

            foreach (var group in jobGroups)
            {
                var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(group));
                foreach (var jobKey in jobKeys)
                {
                    var detail = await scheduler.GetJobDetail(jobKey);
                    var triggers = await scheduler.GetTriggersOfJob(jobKey);
                    
                    // Assume mostly one trigger per job for simplicity in this MVP
                    foreach (var trigger in triggers)
                    {
                        var state = await scheduler.GetTriggerState(trigger.Key);
                        var cronTrigger = trigger as ICronTrigger;
                        
                        result.Add(new JobStatusDto
                        {
                            Group = group,
                            Name = jobKey.Name,
                            Description = detail?.Description ?? string.Empty,
                            Status = state.ToString(),
                            NextFireTime = trigger.GetNextFireTimeUtc()?.LocalDateTime,
                            PreviousFireTime = trigger.GetPreviousFireTimeUtc()?.LocalDateTime,
                            TriggerState = state.ToString(),
                            CronExpression = cronTrigger?.CronExpressionString ?? "N/A"
                        });
                    }
                }
            }

            return result;
        }

        public async Task TriggerJobAsync(string jobName, string groupName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.TriggerJob(new JobKey(jobName, groupName));
            _logger.LogInformation("Triggered job {Group}.{Name}", groupName, jobName);
        }

        public async Task PauseJobAsync(string jobName, string groupName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.PauseJob(new JobKey(jobName, groupName));
            _logger.LogInformation("Paused job {Group}.{Name}", groupName, jobName);
        }

        public async Task ResumeJobAsync(string jobName, string groupName)
        {
            var scheduler = await _schedulerFactory.GetScheduler();
            await scheduler.ResumeJob(new JobKey(jobName, groupName));
            _logger.LogInformation("Resumed job {Group}.{Name}", groupName, jobName);
        }
    }
}
