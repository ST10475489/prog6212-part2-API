using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    // Turns a list of enrolments into result rows, using simple separate queries
    public class ResultBuilder
    {
        private readonly AppDbContext _db;

        public ResultBuilder(AppDbContext db)
        {
            _db = db;
        }

        public List<ResultDto> Build(List<Enrolment> enrolments)
        {
            var rows = new List<ResultDto>();
            if (enrolments.Count == 0) return rows;

            var enrolmentIds = enrolments.Select(e => e.EnrolmentId).ToList();
            var results = _db.Results.Where(r => enrolmentIds.Contains(r.EnrolmentId)).ToList();

            var categoryIds = enrolments.Select(e => e.CategoryId).Distinct().ToList();
            var categories = _db.Categories.Where(c => categoryIds.Contains(c.CategoryId)).ToList();

            var eventIds = categories.Select(c => c.EventId).Distinct().ToList();
            var events = _db.Events.Where(e => eventIds.Contains(e.EventId)).ToList();

            var userIds = enrolments.Select(e => e.ParticipantId).Distinct().ToList();
            var users = _db.Users.Where(u => userIds.Contains(u.UserId)).ToList();

            foreach (var result in results)
            {
                var enrolment = enrolments.First(e => e.EnrolmentId == result.EnrolmentId);
                var category = categories.First(c => c.CategoryId == enrolment.CategoryId);
                var raceEvent = events.First(e => e.EventId == category.EventId);
                var user = users.First(u => u.UserId == enrolment.ParticipantId);

                var row = new ResultDto();
                row.ResultId = result.ResultId;
                row.EnrolmentId = result.EnrolmentId;
                row.ParticipantName = user.FullName;
                row.EventName = raceEvent.EventName;
                row.CategoryName = category.CategoryName;
                row.FinishTime = result.FinishTime;
                row.Position = result.Position;
                rows.Add(row);
            }

            return rows.OrderBy(r => r.CategoryName).ThenBy(r => r.Position).ToList();
        }
    }
}
