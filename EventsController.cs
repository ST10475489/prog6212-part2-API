using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/events")]
    public class EventsController : ApiControllerBase
    {
        private readonly AppDbContext _db;

        public EventsController(AppDbContext db)
        {
            _db = db;
        }

//listing all upcoming events
        /// <summary>Lists all upcoming events.</summary>
        /// <remarks>Public. Returns events dated today or later, soonest first.</remarks>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<RaceEvent>), StatusCodes.Status200OK)]
        public IActionResult GetEvents()
        {
            var events = _db.Events
                .Where(e => e.EventDate >= DateTime.Today)
                .OrderBy(e => e.EventDate)
                .ToList();
            return Ok(events);
        }

/lists the event by searching it by ID
        /// <summary>Gets one event by id.</summary>
        /// <remarks>Public. Returns the event details. Use /route for the route data.</remarks>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RaceEvent), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetEvent(int id)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            return Ok(raceEvent);
        }

        /// <summary>Creates a new event.</summary>
        /// <remarks>Organiser only. Expects eventName, eventDate, location, description, distance (km) and eventType (run, walk or cycle). Returns the new event id.</remarks>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateEvent([FromBody] EventRequest request)
        {
            string error = ValidateEvent(request);
            if (error != null) return BadRequest(error);

            var raceEvent = new RaceEvent();
            raceEvent.OrganiserId = CurrentUserId();
            Fill(raceEvent, request);

            _db.Events.Add(raceEvent);
            _db.SaveChanges();

            return StatusCode(201, new { eventId = raceEvent.EventId });
        }

        /// <summary>Updates an existing event.</summary>
        /// <remarks>Organiser who owns the event only. Same body as create. Returns the updated event.</remarks>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(RaceEvent), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEvent(int id, [FromBody] EventRequest request)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            string error = ValidateEvent(request);
            if (error != null) return BadRequest(error);

            Fill(raceEvent, request);
            _db.SaveChanges();

            return Ok(raceEvent);
        }

        /// <summary>Deletes an event and everything that depends on it.</summary>
        /// <remarks>Organiser who owns the event only. Also removes its categories, route, enrolments and results. Returns no content.</remarks>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteEvent(int id)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            var categories = _db.Categories.Where(c => c.EventId == id).ToList();
            var categoryIds = categories.Select(c => c.CategoryId).ToList();

            var enrolments = _db.Enrolments.Where(e => categoryIds.Contains(e.CategoryId)).ToList();
            var enrolmentIds = enrolments.Select(e => e.EnrolmentId).ToList();

            var results = _db.Results.Where(r => enrolmentIds.Contains(r.EnrolmentId)).ToList();
            var routes = _db.Routes.Where(r => r.EventId == id).ToList();

            _db.Results.RemoveRange(results);
            _db.Enrolments.RemoveRange(enrolments);
            _db.Categories.RemoveRange(categories);
            _db.Routes.RemoveRange(routes);
            _db.Events.Remove(raceEvent);
            _db.SaveChanges();

            return NoContent();
        }

        /// <summary>Gets the route and map data for an event.</summary>
        /// <remarks>Public. Returns the route object.</remarks>
        [HttpGet("{id}/route")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(EventRoute), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetRoute(int id)
        {
            var route = _db.Routes.FirstOrDefault(r => r.EventId == id);
            if (route == null) return NotFound("No route captured for this event.");
            return Ok(route);
        }

        /// <summary>Adds the route for an event (extra endpoint so routes can be created).</summary>
        /// <remarks>Organiser who owns the event only. Expects routeName, startPoint, endPoint and mapUrl. One route per event.</remarks>
        [HttpPost("{id}/route")]
        [Authorize(Roles = "Organiser")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public IActionResult CreateRoute(int id, [FromBody] RouteRequest request)
        {
            var raceEvent = _db.Events.Find(id);
            if (raceEvent == null) return NotFound("Event does not exist.");
            if (raceEvent.OrganiserId != CurrentUserId()) return Forbid();

            if (request == null || string.IsNullOrWhiteSpace(request.RouteName))
            {
                return BadRequest("Route name is required.");
            }

            if (_db.Routes.Any(r => r.EventId == id))
            {
                return Conflict("This event already has a route.");
            }

            var route = new EventRoute();
            route.EventId = id;
            route.RouteName = request.RouteName;
            route.StartPoint = request.StartPoint;
            route.EndPoint = request.EndPoint;
            route.MapUrl = request.MapUrl;

            _db.Routes.Add(route);
            _db.SaveChanges();

            return StatusCode(201, new { routeId = route.RouteId });
        }

        private string ValidateEvent(EventRequest request)
        {
            if (request == null) return "Request body is required.";
            if (string.IsNullOrWhiteSpace(request.EventName)) return "Event name is required.";
            if (string.IsNullOrWhiteSpace(request.Location)) return "Location is required.";
            if (request.EventDate == default(DateTime)) return "Event date is required.";
            if (request.Distance <= 0) return "Distance must be greater than 0.";

            string type = (request.EventType ?? "").Trim().ToLower();
            if (type != "run" && type != "walk" && type != "cycle")
            {
                return "Event type must be run, walk or cycle.";
            }

            return null;
        }

        private void Fill(RaceEvent raceEvent, EventRequest request)
        {
            raceEvent.EventName = request.EventName.Trim();
            raceEvent.EventDate = request.EventDate.Date;
            raceEvent.Location = request.Location.Trim();
            raceEvent.Description = request.Description;
            raceEvent.Distance = request.Distance;
            raceEvent.EventType = request.EventType.Trim().ToLower();
        }
    }
}
