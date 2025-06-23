using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Models;
using OT.Assessment.App.Models.Requests;
using OT.Assessment.App.Models.Responses;
using OT.Assessment.App.Repositories;
using OT.Assessment.App.Services;

namespace OT.Assessment.App.Controllers
{
    /// <summary>
    /// Handles player-related actions such as submitting wagers and retrieving statistics.
    /// </summary>
    [ApiController]
    [Route("api/Player")]
    public class PlayerController : ControllerBase
    {
        private readonly IRabbitMqService _rabbit;
        private readonly IWagerRepository _wagerRepository;
        private readonly IUserStatsRepository _userStatsRepository;
        public PlayerController(IRabbitMqService rabbit, IWagerRepository wagerRepository, IUserStatsRepository userStatsRepository)
        {
            _rabbit = rabbit;
            _wagerRepository = wagerRepository; // Assuming WagerRepository is implemented and available
            _userStatsRepository=userStatsRepository;
        }

        //POST api/player/casinowager
        /// <summary>
        /// Publishes a casino wager event to RabbitMQ.
        /// </summary>
        /// <param name="wagerEvent">The wager event payload.</param>
        /// <returns>A confirmation of the publish action.</returns>
        [Authorize]
        [HttpPost("CasinoWager")]
        public async Task<IActionResult> PostCasinoWagerAsync([FromBody] CasinoWager wagerEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _rabbit.PublishAsync(wagerEvent);

            return Ok(new { status = "Published to RabbitMQ" });
        }

        //GET api/player/{playerId}/wagers
        /// <summary>
        /// Retrieves a paginated list of casino wagers for a specific player.
        /// </summary>
        /// <param name="playerId">The unique identifier of the player.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>A paginated response of casino wagers.</returns>
        [Authorize]
        [HttpGet("{playerId:guid}/casino")]
        public async Task<IActionResult> GetCasinoWagers(Guid playerId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize <= 0 || pageSize > 100)
                return BadRequest("Invalid pagination values.");

            var (wagers, total) = await _wagerRepository.GetCasinoWagersForPlayerAsync(playerId, page, pageSize);

            var response = new PaginatedResponse<WagerDto>
            {
                Data = wagers,
                Page = page,
                PageSize = pageSize,
                Total = total
            };

            return Ok(response);
        }

        //GET api/player/topSpenders?count=10
        ///// <summary>
        /// Retrieves the top players based on total spending.
        /// </summary>
        /// <param name="count">The number of top spenders to return (default is 10).</param>
        /// <returns>A list of top spenders.</returns>
        [Authorize]
        [HttpGet("topSpenders")]
        public async Task<IActionResult> GetTopSpenders([FromQuery] int count = 10)
        {
            if (count <= 0 || count > 100)
                return BadRequest("Count must be between 1 and 100.");

            var result = await _userStatsRepository.GetTopSpendersAsync(count);
            return Ok(result);
        }
    }
}
