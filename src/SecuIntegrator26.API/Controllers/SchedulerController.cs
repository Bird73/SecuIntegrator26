using Microsoft.AspNetCore.Mvc;
using SecuIntegrator26.Core.Interfaces;
using SecuIntegrator26.Shared.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecuIntegrator26.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulerController : ControllerBase
    {
        private readonly ISchedulerManagementService _schedulerService;

        public SchedulerController(ISchedulerManagementService schedulerService)
        {
            _schedulerService = schedulerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<JobStatusDto>>> GetJobs()
        {
            return Ok(await _schedulerService.GetAllJobsAsync());
        }

        [HttpPost("trigger")]
        public async Task<ActionResult> TriggerJob([FromQuery] string name, [FromQuery] string group)
        {
            await _schedulerService.TriggerJobAsync(name, group);
            return Ok();
        }

        [HttpPost("pause")]
        public async Task<ActionResult> PauseJob([FromQuery] string name, [FromQuery] string group)
        {
            await _schedulerService.PauseJobAsync(name, group);
            return Ok();
        }

        [HttpPost("resume")]
        public async Task<ActionResult> ResumeJob([FromQuery] string name, [FromQuery] string group)
        {
            await _schedulerService.ResumeJobAsync(name, group);
            return Ok();
        }
    }
}
