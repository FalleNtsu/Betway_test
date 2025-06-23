using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Models.Requests;
using OT.Assessment.App.Services;

namespace OT.Assessment.App.Controllers
{
    public class AdminController : ControllerBase
    {
        private readonly IRabbitMqService _rabbitMqService;
        public AdminController(IRabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        /// <summary>
        /// Retrieves up to <paramref name="maxCount"/> messages from the Dead-Letter Queue (DLQ).
        /// </summary>
        /// <param name="maxCount">The maximum number of DLQ messages to retrieve. Default is 50.</param>
        /// <returns>A list of <see cref="CasinoWager"/> messages from the DLQ.</returns>
        /// <response code="200">Returns a list of dead-lettered messages.</response>
        /// <response code="401">Unauthorized. The caller is not authenticated.</response>
        [Authorize]
        [HttpGet("deadletter")]
        public async Task<IActionResult> GetDeadLetterMessages([FromQuery] int maxCount = 50)
        {
            var messages = await _rabbitMqService.GetDeadLetterMessagesAsync(maxCount);
            if (messages == null || !messages.Any())
            {
                return NotFound("No messages found in the Dead-Letter Queue.");
            }
            else
            {
                return Ok(messages);
            }
        }

        /// <summary>
        /// Replays up to <paramref name="maxCount"/> messages from the Dead-Letter Queue by republishing them to the main queue.
        /// </summary>
        /// <param name="maxCount">The maximum number of DLQ messages to replay. Default is 50.</param>
        /// <returns>A result indicating how many messages were replayed.</returns>
        /// <response code="200">Returns the number of successfully replayed messages.</response>
        /// <response code="401">Unauthorized. The caller is not authenticated.</response>
        [Authorize]
        [HttpPost("deadletter/replay")]
        public async Task<IActionResult> ReplayDeadLetterMessages([FromQuery] int maxCount = 50)
        {
            var messages = await _rabbitMqService.GetDeadLetterMessagesAsync(maxCount);

            foreach (var msg in messages)
            {
                await _rabbitMqService.PublishAsync(msg);
            }

            return Ok(new { replayedCount = messages.Count });
        }
    }
}
