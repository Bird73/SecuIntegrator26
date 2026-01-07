using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using SecuIntegrator26.Core.Interfaces;
using SecuIntegrator26.Shared.DTOs;
using SecuIntegrator26.Core.Entities;
using SecuIntegrator26.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecuIntegrator26.Services
{
    public class SchedulerManagementService : ISchedulerManagementService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IFileService _fileService;
        private readonly ILogger<SchedulerManagementService> _logger;
        private const string ConfigFileName = "scheduler_config.json";

        public SchedulerManagementService(ISchedulerFactory schedulerFactory, IFileService fileService, ILogger<SchedulerManagementService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _fileService = fileService;
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

        public async Task<JobScheduleConfig> GetConfigAsync()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            // Try load from file
            var content = await _fileService.ReadTextAsync(ConfigFileName);
            if (!string.IsNullOrEmpty(content))
            {
                try
                {
                    return JsonSerializer.Deserialize<JobScheduleConfig>(content) ?? new JobScheduleConfig();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load scheduler config, falling back to runtime defaults.");
                }
            }

            // Fallback: Generate config from current runtime state
            var jobs = await GetAllJobsAsync();
            var config = new JobScheduleConfig();
            
            // Group by Job Name/Group to merge triggers
            var groupedJobs = jobs.GroupBy(j => new { j.Name, j.Group });

            foreach (var grp in groupedJobs)
            {
                var mainJob = grp.First();
                // Get real triggers from scheduler to be sure (GetAllJobsAsync might have flattened them or not)
                var jobKey = new JobKey(mainJob.Name, mainJob.Group);
                var triggers = await scheduler.GetTriggersOfJob(jobKey);
                
                var cronExpressions = new List<string>();
                foreach (var t in triggers)
                {
                    if (t is ICronTrigger ct) cronExpressions.Add(ct.CronExpressionString);
                }

                config.Jobs.Add(new JobSetting
                {
                    JobName = mainJob.Name,
                    GroupName = mainJob.Group,
                    CronExpression = cronExpressions.FirstOrDefault() ?? "",
                    CronExpressions = cronExpressions,
                    IsEnabled = mainJob.TriggerState != "Paused", // If any trigger is active, we consider job active? Or check trigger state.
                    Description = mainJob.Description
                });
            }
            return config;
        }

        public async Task UpdateConfigAsync(JobScheduleConfig config)
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            foreach (var setting in config.Jobs)
            {
                var jobKey = new JobKey(setting.JobName, setting.GroupName);
                if (!await scheduler.CheckExists(jobKey)) continue;

                // Handle Enable/Disable
                if (setting.IsEnabled)
                {
                    await scheduler.ResumeJob(jobKey);
                }
                else
                {
                    await scheduler.PauseJob(jobKey);
                }

                // Handle Cron Update (Multiple Triggers)
                // Strategy: Remove all existing Cron triggers for this job and re-create them from config
                var existingTriggers = await scheduler.GetTriggersOfJob(jobKey);
                
                // If CronExpressions list is empty but CronExpression has value, use it (backwards compat)
                if (setting.CronExpressions.Count == 0 && !string.IsNullOrEmpty(setting.CronExpression))
                {
                    setting.CronExpressions.Add(setting.CronExpression);
                }

                // Remove existing triggers (All triggers, including SimpleTriggers, to ensure clean state)
                var triggersToRemove = existingTriggers.ToList();
                if (triggersToRemove.Any())
                {
                    await scheduler.UnscheduleJobs(triggersToRemove.Select(t => t.Key).ToList());
                }

                // Add new triggers
                for (int i = 0; i < setting.CronExpressions.Count; i++)
                {
                    var cron = setting.CronExpressions[i];
                    if (string.IsNullOrWhiteSpace(cron)) continue;

                    var triggerKey = new TriggerKey($"{setting.JobName}_Trigger_{i}", setting.GroupName);
                    
                    var newTrigger = TriggerBuilder.Create()
                        .WithIdentity(triggerKey)
                        .WithCronSchedule(cron)
                        .ForJob(jobKey)
                        .Build();

                    await scheduler.ScheduleJob(newTrigger);
                    _logger.LogInformation("Scheduled {Job} with {Cron}", setting.JobName, cron);
                }

                // Apply Paused state if needed (after scheduling)
                if (!setting.IsEnabled)
                {
                    await scheduler.PauseJob(jobKey);
                }
                else
                {
                    await scheduler.ResumeJob(jobKey);
                }
            }

            // Save to file
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            await _fileService.SaveTextAsync(ConfigFileName, json);
            await _fileService.SaveTextAsync(ConfigFileName, json);
        }

        public async Task RestoreConfigAsync()
        {
            try
            {
                // Only restore if config file exists
                // GetConfigAsync handles reading from file or fallback to current state.
                // If we get fallback state (Program.cs defaults), reapplying it is harmless.
                // If we get file state, we override Program.cs defaults.
                var config = await GetConfigAsync();
                await UpdateConfigAsync(config);
                _logger.LogInformation("Restored scheduler configuration from file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restore scheduler configuration.");
            }
        }
    }
}
