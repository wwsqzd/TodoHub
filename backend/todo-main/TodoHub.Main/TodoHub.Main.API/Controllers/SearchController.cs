using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TodoHub.Main.Core.Interfaces;
using Serilog;

namespace TodoHub.Main.API.Controllers
{
    [Route("api/search")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IElastikSearchService _searchService;   
        public SearchController(IElastikSearchService searchService) 
        {
            _searchService = searchService;
        }
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> SearchDocuments([FromQuery] string q, CancellationToken ct)
        {
            Log.Information("Search in Documents starting in Controller");
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query is empty");

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");
            
            var res = await _searchService.SearchDocuments(userId, q, ct);
            if (!res.Success)
            {
                return BadRequest(res.Error);
            }
            return Ok(res.Value);
        }

        [HttpPost("create-index")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateIndex()
        {
            Log.Information("Create Index starting in Controller");
            var res = await _searchService.CreateIndex();
            if (!res.Success)
            {
                return BadRequest(res.Error);
            }
            return Ok(res.Value);
        }


        [HttpPost("reindex")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReIndex()
        {
            Log.Information("Reindex starting in Controller");
            var res = await _searchService.ReIndex();
            if (!res.Success)
            {
                return BadRequest(res.Error);
            }
            return Ok(res.Value);
        }
    }
}
