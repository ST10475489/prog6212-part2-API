using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class CategoriesController : ApiControllerBase
    {
        private readonly AppDbContext _db;

        public CategoriesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Lists all categories for an event.</summary>
        /// <remarks>Public. Returns an array of categories.</remarks>
        [HttpGet("api/events/{id}/categories")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<Category>), StatusCodes.Status200OK)]
        public IActionResult GetCategories(int id)
        {
            var categories = _db.Categories.Where(c => c.EventId == id).ToList();
            return Ok(categories);
        }

        /// <summary>Adds a category to an event.</summary>
        /// <remarks>Organiser who owns the event only. Expects categoryName, distanceKm and maxParticipants. Returns the new category id.</remarks>
        [HttpPost("api/events/{id}/categories")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CreateCategory(int id, [FromBody] CategoryRequest request)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            string error = Validate(request);
            if (error != null) return BadRequest(error);

            var category = new Category();
            category.EventId = id;
            category.CategoryName = request.CategoryName.Trim();
            category.Distance = request.DistanceKm;
            category.Participants = request.MaxParticipants;

            _db.Categories.Add(category);
            _db.SaveChanges();

            return StatusCode(201, new { categoryId = category.CategoryId });
        }

        /// <summary>Updates an existing category.</summary>
        /// <remarks>Organiser who owns the event only. Same body as create. Returns the updated category.</remarks>
        [HttpPut("api/categories/{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateCategory(int id, [FromBody] CategoryRequest request)
        {
            var category = _db.Categories.Find(id);
            if (category == null) return NotFound("Category does not exist.");

            var raceEvent = _db.Events.Find(category.EventId);
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            string error = Validate(request);
            if (error != null) return BadRequest(error);

            category.CategoryName = request.CategoryName.Trim();
            category.Distance = request.DistanceKm;
            category.Participants = request.MaxParticipants;
            _db.SaveChanges();

            return Ok(category);
        }

        /// <summary>Deletes a category and its enrolments.</summary>
        /// <remarks>Organiser who owns the event only. Also removes enrolments and results in the category. Returns no content.</remarks>
        [HttpDelete("api/categories/{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteCategory(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null) return NotFound("Category does not exist.");

            var raceEvent = _db.Events.Find(category.EventId);
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            var enrolments = _db.Enrolments.Where(e => e.CategoryId == id).ToList();
            var enrolmentIds = enrolments.Select(e => e.EnrolmentId).ToList();
            var results = _db.Results.Where(r => enrolmentIds.Contains(r.EnrolmentId)).ToList();

            _db.Results.RemoveRange(results);
            _db.Enrolments.RemoveRange(enrolments);
            _db.Categories.Remove(category);
            _db.SaveChanges();

            return NoContent();
        }

        private string Validate(CategoryRequest request)
        {
            if (request == null) return "Request body is required.";
            if (string.IsNullOrWhiteSpace(request.CategoryName)) return "Category name is required.";
            if (request.DistanceKm <= 0) return "Distance must be greater than 0.";
            if (request.MaxParticipants <= 0) return "Max participants must be greater than 0.";
            return null;
        }
    }
}
