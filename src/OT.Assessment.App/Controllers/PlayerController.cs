using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OT.Assessment.App.Models;
using OT.Assessment.App.Models.Requests;
using OT.Assessment.App.Models.Responses;
using OT.Assessment.App.Repositories;
using OT.Assessment.App.Services;

namespace OT.Assessment.App.Controllers
{
  
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
        //[Authorize]
        [HttpPost("CasinoWager")]
        public async Task<IActionResult> PostCasinoWagerAsync([FromBody] CasinoWager wagerEvent)
        {
            if (wagerEvent == null)
                return BadRequest("Invalid payload");

            await _rabbit.PublishAsync(wagerEvent);

            return Ok(new { status = "Published to RabbitMQ" });
        }

        //GET api/player/{playerId}/wagers
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
