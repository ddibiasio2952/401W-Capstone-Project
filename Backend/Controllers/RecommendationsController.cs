using FalveyInsuranceGroup.Backend.Dtos;
using FalveyInsuranceGroup.Backend.Models;
using FalveyInsuranceGroup.Backend.Services;
using FalveyInsuranceGroup.Db;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace FalveyInsuranceGroum.Backend.Controllers
{
    /// <summary>
    /// Handles operations related to recommendation data
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class RecommendationsController : ControllerBase
    {
        private readonly FalveyInsuranceGroupContext _context;
        private readonly InputService _service;

        public RecommendationsController(FalveyInsuranceGroupContext context, InputService service)
        {
            _context = context;
            _service = service;
        }

        /// GET: api/recommendations

        /// <summary>
        /// Gets a list of recommendations
        /// </summary>
        /// <returns>A list of recommendations</returns>
        [HttpGet]
        public async Task<List<RecommendationDto>> getRecommendations()
        {
            var recommendation = await _context.Recommendations
                .Select(m => new RecommendationDto
                {
                    recommendation_id = m.recommendation_id,
                    user_id = m.user_id,
                    policy_id = m.policy_id,
                    recommendation_text = m.recommendation_text,
                    created_at = m.created_at
                }).
            ToListAsync();
            return recommendation;
        }

        /// GET: api/recommendations/id

        /// <summary>
        /// Gets a recommendation by ID
        /// </summary>
        /// <param name="id"> The id of the recommendation to retrieve</param>
        /// <returns>A recommendation dto based on provided ID</returns>
        /// <response code="200">Returns the recommendation</response>
        /// <response code="404">If the recommendation is not found</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<RecommendationDto>> getRecommendationById(int id)
        {
            var recommendation = await _context.Recommendations.Where(m => m.recommendation_id == id).FirstOrDefaultAsync();

            // Verify recommendation exists.
            if (recommendation == null)
            {
                return NotFound($"Recommendation with ID {id} not found");
            }

            return createRecommendationDto(recommendation);
        }

        /// GET: api/recommendations/search?policy_id

        /// <summary>
        /// Gets a list of recommendations based on a Policy ID query
        /// </summary>
        /// <returns>A list of recommendations based on a Policy ID query</returns>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RecommendationDto>>> getRecommendationsByPolicy([FromQuery] int? policy_id)
        {
            var recommendation = _context.Recommendations
                .Select(r => new RecommendationDto
                {
                    recommendation_id = r.recommendation_id,
                    user_id = r.user_id,
                    policy_id = r.policy_id,
                    recommendation_text = r.recommendation_text,
                    created_at = r.created_at
                });

            if (policy_id.HasValue)
            {
                recommendation = recommendation.Where(r => r.policy_id == policy_id.Value);
            }

            var results = await recommendation.ToListAsync();
            return Ok(results);
        }

        /// POST: api/recommendations

        /// <summary>
        /// Adds a new recommendation
        /// </summary>
        /// <param name="new_recommendation">Recommendation object to add</param>
        /// <returns>The new record</returns>
        /// <response code="201">Record added successfully</response>
        /// <response code="400">Validation failed</response>
        [HttpPost]
        public async Task<ActionResult> addRecommendation([FromBody] RecommendationDto dto)
        {
            if (!await _service.checkPolicy(dto.policy_id))
            {
                return BadRequest("The policy does not exist.");
            }

            if (dto.recommendation_text.Length > 500)
            {
                return BadRequest("The text exceeds the character limit of 500.");
            }

            var new_recommendation = new Recommendation
            {
                recommendation_id = dto.recommendation_id,
                user_id = dto.user_id,
                policy_id = dto.policy_id,
                recommendation_text = dto.recommendation_text,
                created_at = dto.created_at
            };

            _context.Recommendations.Add(new_recommendation);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(getRecommendationById), new { id = new_recommendation.recommendation_id }, createRecommendationDto(new_recommendation));
        }

        /// PUT: api/recommendations/id

        /// <summary>
        ///  Updates an existing recommendation by ID
        /// </summary>
        /// <param name="id">The recommendation to update</param>
        /// <param name="updated_recommendation">Recommendation object that holds the new details</param>
        /// <returns>No content</returns>
        /// <response code="204">The update was a success</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Record not found</response>
        [HttpPut("{id}")]
        public async Task<ActionResult> updateRecommendation(int id, [FromBody] RecommendationDto updated_recommendation)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);

            // Check if Employee ID exists in system
            if (!await _service.checkPolicy(updated_recommendation.policy_id))
            {
                return BadRequest("The policy does not exist.");
            }

            // Validate created at DateTime.
            if (updated_recommendation.created_at > DateTime.Now)
            {
                return BadRequest("Creation date cannot be future.");
            }

            // Validate recommendation is not null.
            if (recommendation == null)
            {
                return NotFound($"Recommendation with ID {id} not found");
            }

            // Validate text length.
            if (updated_recommendation.recommendation_text.Length > 500)
            {
                return BadRequest("The text exceeds the character limit of 500.");
            }

            // Update fields
            recommendation.user_id = updated_recommendation.user_id;
            recommendation.policy_id = updated_recommendation.policy_id;
            recommendation.recommendation_text = updated_recommendation.recommendation_text;
            recommendation.created_at = updated_recommendation.created_at;

            await _context.SaveChangesAsync();

            return Ok(recommendation);
        }

        /// DELETE: api/recommendations/id

        /// <summary>
        /// Deletes an existing recommendation
        /// </summary>
        /// <param name="id">The recommendation to delete</param>
        /// <returns>No content</returns>
        /// <response code="204">Deletion was a success</response>
        /// <response code="404">Recommendation not found</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteRecommendation(int id)
        {
            var recommendation = await _context.Recommendations.FindAsync(id);

            // Check if recommendation exists.
            if (recommendation == null)
            {
                return NotFound($"Recommendation with ID {id} not found");
            }

            _context.Recommendations.Remove(recommendation);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        /// <summary>
        /// Creates a dto using a recommendation entity model
        /// </summary>
        /// <param name="p">The recommendation entity model</param>
        /// <returns>A recommendation dto with the only necessary information</returns>
        private RecommendationDto createRecommendationDto(Recommendation r)
        {
            return new RecommendationDto
            {
                recommendation_id = r.recommendation_id,
                user_id = r.user_id,
                policy_id = r.policy_id,
                recommendation_text = r.recommendation_text,
                created_at = r.created_at,
            };
        }
    }
}
