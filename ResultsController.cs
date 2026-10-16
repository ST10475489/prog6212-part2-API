using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class ResultsController : ApiControllerBase
    {
        private readonly AppDbContext _db;

        public ResultsController(AppDbContext db)
        {
            _db = db;
        }

//captures the finished results
        /// <summary>Captures a finish time and position for an enrolment.</summary>
        /// <remarks>Organiser who owns the event only. Expects finishTime (hh:mm:ss) and position. Fails with 409 if a result already exists.</remarks>
        [HttpPost("api/enrolments/{id}/results")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CaptureResult(int id, [FromBody] ResultRequest request)
        {
            var enrolment = _db.Enrolments.Find(id);
            if (enrolment == null) return NotFound("Enrolment does not exist.");

            var category = _db.Categories.Find(enrolment.CategoryId);
            var raceEvent = _db.Events.Find(category.EventId);
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            if (request == null || string.IsNullOrWhiteSpace(request.FinishTime) || request.Position <= 0)
            {
                return BadRequest("Finish time and a position greater than 0 are required.");
            }

            TimeSpan parsed;
            if (!TimeSpan.TryParse(request.FinishTime, out parsed))
            {
                return BadRequest("Finish time must look like 01:45:30.");
            }

            if (_db.Results.Any(r => r.EnrolmentId == id))
            {
                return Conflict("A result has already been captured for this enrolment.");
            }

            var result = new RaceResult();
            result.EnrolmentId = id;
            result.FinishTime = request.FinishTime;
            result.Position = request.Position;

            _db.Results.Add(result);
            _db.SaveChanges();

            return StatusCode(201, new { resultId = result.ResultId });
        }

//gets public results for n event of all categories
        /// <summary>Gets the public results for an event across all categories.</summary>
        /// <remarks>Public. Returns an array of results ordered by category then position.</remarks>
        [HttpGet("api/events/{id}/results")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEventResults(int id)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");

            var categoryIds = _db.Categories.Where(c => c.EventId == id).Select(c => c.CategoryId).ToList();
            var enrolments = _db.Enrolments.Where(e => categoryIds.Contains(e.CategoryId)).ToList();

            var builder = new ResultBuilder(_db);
            return Ok(builder.Build(enrolments));
        }

        /// <summary>Gets the public results for one category.</summary>
        /// <remarks>Public. Returns an array of results ordered by position.</remarks>
        [HttpGet("api/categories/{id}/results")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetCategoryResults(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null) return NotFound("Category does not exist.");

            var enrolments = _db.Enrolments.Where(e => e.CategoryId == id).ToList();

            var builder = new ResultBuilder(_db);
            return Ok(builder.Build(enrolments));
        }
    }
}
