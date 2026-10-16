namespace RaceDay.Api.Dtos
{
//for registration
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }

    //for login
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    //event information
    public class EventRequest
    {
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public decimal Distance { get; set; }
        public string EventType { get; set; }   // run, walk or cycle
    }

    public class CategoryRequest
    {
        public string CategoryName { get; set; }
        public decimal DistanceKm { get; set; }
        public int MaxParticipants { get; set; }
    }

    public class RouteRequest
    {
        public string RouteName { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string MapUrl { get; set; }
    }

    public class ResultRequest
    {
        public string FinishTime { get; set; }   // e.g. 01:45:30
        public int Position { get; set; }
    }
}
