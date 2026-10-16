using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    public class EnrolmentsController : ApiControllerBase
    {
        private readonly AppDbContext _db;

        public EnrolmentsController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Enters the logged-in participant into a category.</summary>
        /// <remarks>Participant only. No body needed - the participant comes from the token. Fails with 409 if the category is full or the participant is already enrolled.</remarks>
        [HttpPost("api/categories/{id}/enrolments")]
        [Authorize(Roles = "Participant")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult Enrol(int id)
        {
            var category = _db.Categories.Find(id);
            if (category == null) return NotFound("Category does not exist.");

            int userId = CurrentUserId();

            if (_db.Enrolments.Any(e => e.CategoryId == id && e.ParticipantId == userId))
            {
                return Conflict("You are already enrolled in this category.");
            }

            int taken = _db.Enrolments.Count(e => e.CategoryId == id);
            if (taken >= category.Participants)
            {
                return Conflict("This category is full.");
            }

            var enrolment = new Enrolment();
            enrolment.ParticipantId = userId;
            enrolment.CategoryId = id;
            enrolment.EnrolmentDate = DateTime.UtcNow;
            enrolment.Status = "Confirmed";

            _db.Enrolments.Add(enrolment);
            _db.SaveChanges();

            return StatusCode(201, new { enrolmentId = enrolment.EnrolmentId, status = enrolment.Status });
        }

        /// <summary>Lists a participant's current and past enrolments.</summary>
        /// <remarks>Any role, own id only. Returns an array of enrolments with event and category names.</remarks>
        [HttpGet("api/users/{id}/enrolments")]
        [ProducesResponseType(typeof(List<EnrolmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetUserEnrolments(int id)
        {
            if (id != CurrentUserId()) return Forbid();

            var enrolments = _db.Enrolments.Where(e => e.ParticipantId == id).ToList();
            return Ok(BuildDtos(enrolments));
        }

        /// <summary>Lists all enrolments for an event.</summary>
        /// <remarks>Organiser who owns the event only (extra endpoint required by the brief).</remarks>
        [HttpGet("api/events/{id}/enrolments")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(List<EnrolmentDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEventEnrolments(int id)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            var categoryIds = _db.Categories.Where(c => c.EventId == id).Select(c => c.CategoryId).ToList();
            var enrolments = _db.Enrolments.Where(e => categoryIds.Contains(e.CategoryId)).ToList();
            return Ok(BuildDtos(enrolments));
        }

        /// <summary>Cancels an enrolment before the event takes place.</summary>
        /// <remarks>The enrolled participant or the event's organiser. Returns no content.</remarks>
        [HttpDelete("api/enrolments/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult CancelEnrolment(int id)
        {
            var enrolment = _db.Enrolments.Find(id);
            if (enrolment == null) return NotFound("Enrolment does not exist.");

            var category = _db.Categories.Find(enrolment.CategoryId);
            var raceEvent = _db.Events.Find(category.EventId);

            bool isOwner = CurrentRole() == Roles.Participant && enrolment.ParticipantId == CurrentUserId();
            bool isOrganiser = CurrentRole() == Roles.Organiser && raceEvent.OrganiserId == CurrentUserId();
            if (!isOwner && !isOrganiser) return Forbid();

            if (raceEvent.EventDate < DateTime.Today)
            {
                return BadRequest("The event has already taken place.");
            }

            var results = _db.Results.Where(r => r.EnrolmentId == id).ToList();
            _db.Results.RemoveRange(results);
            _db.Enrolments.Remove(enrolment);
            _db.SaveChanges();

            return NoContent();
        }

        private List<EnrolmentDto> BuildDtos(List<Enrolment> enrolments)
        {
            var rows = new List<EnrolmentDto>();
            if (enrolments.Count == 0) return rows;

            var categoryIds = enrolments.Select(e => e.CategoryId).Distinct().ToList();
            var categories = _db.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToList();

            var eventIds = categories.Select(c => c.EventId).Distinct().ToList();
            var events = _db.Events.Where(e => eventIds.Contains(e.EventId)).ToList();

            var userIds = enrolments.Select(e => e.ParticipantId).Distinct().ToList();
            var users = _db.Users.Where(u => userIds.Contains(u.UserId)).ToList();

            foreach (var enrolment in enrolments)
            {
                var category = categories.First(c => c.CategoryId == enrolment.CategoryId);
                var raceEvent = events.First(e => e.EventId == category.EventId);
                var user = users.First(u => u.UserId == enrolment.ParticipantId);

                var row = new EnrolmentDto();
                row.EnrolmentId = enrolment.EnrolmentId;
                row.ParticipantId = user.UserId;
                row.ParticipantName = user.FullName;
                row.CategoryId = category.CategoryId;
                row.CategoryName = category.CategoryName;
                row.EventId = raceEvent.EventId;
                row.EventName = raceEvent.EventName;
                row.EventDate = raceEvent.EventDate;
                row.EnrolmentDate = enrolment.EnrolmentDate;
                row.Status = enrolment.Status;
                rows.Add(row);
            }

            return rows;
        }
    }
}
